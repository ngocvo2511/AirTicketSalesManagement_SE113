using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Helper;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Services.PaymentGateway;
using AirTicketSalesManagement.Services.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AirTicketSalesManagement.ViewModel.Customer
{
    public partial class BookingHistoryViewModel : BaseViewModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _templateService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAirTicketDbContextService _dbContextFactory;
        private readonly IUserSessionService _userSession;
        private readonly INotificationService _notificationService;


        private readonly CustomerViewModel parent;
        private ObservableCollection<KQLichSuDatVe> rootHistoryBooking;
        [ObservableProperty]
        private DateTime? ngayDatFilter;
        [ObservableProperty]
        private string noiDiFilter;
        [ObservableProperty]
        private string noiDenFilter;
        [ObservableProperty]
        private string? hangHangKhongFilter;
        [ObservableProperty]
        private string bookingStatusFilter;
        [ObservableProperty]
        private ObservableCollection<string> hangHangKhongList = new();
        [ObservableProperty]
        private ObservableCollection<string> sanBayList = new();
        [ObservableProperty]
        private ObservableCollection<string> bookingStatusList;
        public ObservableCollection<string> DiemDiList =>
            new(SanBayList.Where(s => s != NoiDenFilter));

        public ObservableCollection<string> DiemDenList =>
            new(SanBayList.Where(s => s != NoiDiFilter));

        partial void OnNoiDiFilterChanged(string value)
        {
            OnPropertyChanged(nameof(DiemDenList));
        }

        partial void OnNoiDenFilterChanged(string value)
        {
            OnPropertyChanged(nameof(DiemDiList));
        }
        [ObservableProperty]
        private ObservableCollection<KQLichSuDatVe>? historyBooking = new();
        [ObservableProperty]
        private bool isEmpty;
        public NotificationViewModel Notification { get; }

        public BookingHistoryViewModel() { }
        public BookingHistoryViewModel(
             int? idCustomer,
             CustomerViewModel parent,
             IEmailService emailService,
             EmailTemplateService templateService,
             IAirTicketDbContextService dbContextFactory,
             IPaymentGateway paymentGateway,
             IUserSessionService userSession,
             INotificationService notificationService)
        {
            this.parent = parent;
            _emailService = emailService;
            _templateService = templateService;
            _dbContextFactory = dbContextFactory;
            _paymentGateway = paymentGateway;
            _userSession = userSession;
            _notificationService = notificationService;

            // Lấy đúng ViewModel NOTIFICATION dùng chung
            if (notificationService is NotificationService ns)
                Notification = ns.ViewModel;        // <--- IMPORTANT
            else
                Notification = new NotificationViewModel(); // fallback

            ClearExpiredHolds();
        }

        public async Task LoadData(int? idCustomer)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                var HuyVe = await context.Quydinhs.FirstOrDefaultAsync();

                var query = (from datve in context.Datves
                             where datve.MaKh == idCustomer
                             join lichbay in context.Lichbays on datve.MaLb equals lichbay.MaLb
                             join chuyenbay in context.Chuyenbays on lichbay.SoHieuCb equals chuyenbay.SoHieuCb
                             join sbDi in context.Sanbays on chuyenbay.Sbdi equals sbDi.MaSb
                             join sbDen in context.Sanbays on chuyenbay.Sbden equals sbDen.MaSb
                             select new KQLichSuDatVe
                             {
                                 MaVe = datve.MaDv,
                                 MaDiemDi = chuyenbay.Sbdi,
                                 MaDiemDen = chuyenbay.Sbden,
                                 DiemDi = sbDi.ThanhPho + " (" + sbDi.MaSb + "), " + sbDi.QuocGia,
                                 DiemDen = sbDen.ThanhPho + " (" + sbDen.MaSb + "), " + sbDen.QuocGia,
                                 HangHangKhong = chuyenbay.HangHangKhong,
                                 GioDi = lichbay.GioDi,
                                 GioDen = lichbay.GioDen,
                                 LoaiMayBay = lichbay.LoaiMb,
                                 NgayDat = datve.ThoiGianDv,
                                 TrangThai = datve.TtdatVe,
                                 SoLuongKhach = datve.Ctdvs.Count,
                                 QdHuyVe = (HuyVe != null) ? HuyVe.TghuyDatVe : null,
                                 TongTienTT = datve.TongTienTt.Value
                             });

                var result = await query.OrderByDescending(x => x.NgayDat).ToListAsync();

                rootHistoryBooking = new ObservableCollection<KQLichSuDatVe>(result);
                HistoryBooking = new ObservableCollection<KQLichSuDatVe>(result);
                IsEmpty = HistoryBooking.Count == 0;

                var airlineName = await context.Chuyenbays
                    .Select(v => v.HangHangKhong)
                    .Distinct()
                    .ToListAsync();
                HangHangKhongList = new ObservableCollection<string>(airlineName);

                var danhSach = context.Sanbays
                    .AsEnumerable()
                    .Select(sb => $"{sb.ThanhPho} ({sb.MaSb}), {sb.QuocGia}")
                    .OrderBy(display => display)
                    .ToList();
                SanBayList = new ObservableCollection<string>(danhSach);

                BookingStatusList = new ObservableCollection<string>
                {
                    "Tất cả",
                    "Đã thanh toán",
                    "Chưa thanh toán (Tiền mặt)",
                    "Chưa thanh toán (Online)",
                    "Đã hủy"
                };
                BookingStatusFilter = "Tất cả";
            }
            catch
            {
                // có thể log hoặc dùng _notificationService.ShowAsync(...)
            }
        }

        public void ClearExpiredHolds()
        {
            try
            {
                using (var context = new AirTicketDbContext())
                {
                    var quiDinh = context.Quydinhs.FirstOrDefault();
                    int tgHuy = quiDinh?.TghuyDatVe ?? 0;
                    var expiredDatVes = context.Datves
                        .Where(dv => (dv.TtdatVe == "Chưa thanh toán (Online)" || dv.TtdatVe == "Giữ chỗ") &&
                                     (dv.ThoiGianDv < DateTime.Now.AddMinutes(-20) || (dv.MaLbNavigation != null && dv.MaLbNavigation.GioDi != null && DateTime.Now.Date >= dv.MaLbNavigation.GioDi.Value.AddDays(-tgHuy).Date)))
                        .ToList();
                    foreach (var datVe in expiredDatVes)
                    {
                        var chiTiets = context.Ctdvs.Where(ct => ct.MaDv == datVe.MaDv).ToList();

                        var maHvLb = chiTiets.FirstOrDefault()?.MaHvLb;

                        if (maHvLb != null)
                        {
                            var hangVe = context.Hangvetheolichbays
                                .FirstOrDefault(h => h.MaHvLb == maHvLb);

                            if (hangVe != null)
                            {
                                hangVe.SlveConLai += chiTiets.Count;
                            }
                        }

                        context.Ctdvs.RemoveRange(chiTiets);
                        context.Datves.Remove(datVe);
                    }
                    var datVeTienMat = context.Datves.Where(dv => dv.TtdatVe == "Chưa thanh toán (Tiền mặt)"
                        && dv.MaLbNavigation != null && dv.MaLbNavigation.GioDi != null
                        && DateTime.Now.Date >= dv.MaLbNavigation.GioDi.Value.AddDays(-tgHuy).Date)
                        .ToList();
                    foreach (var ve in datVeTienMat)
                    {
                        var chitiet = context.Ctdvs.Where(ct => ct.MaDv == ve.MaDv).ToList();
                        var maHvLb = chitiet.FirstOrDefault()?.MaHvLb;
                        if (maHvLb != null) 
                        {
                            var hangVe = context.Hangvetheolichbays
                                .FirstOrDefault(h => h.MaHvLb == maHvLb);
                            if (hangVe != null)
                            {
                                hangVe.SlveConLai += chitiet.Count;
                            }
                        }
                        ve.TtdatVe = "Đã hủy";
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }

        [RelayCommand]
        private void ShowDetailHistory(KQLichSuDatVe lichSuDatVe)
        {
            parent.CurrentViewModel = new BookingHistoryDetailViewModel(lichSuDatVe, parent, _emailService, _templateService, _dbContextFactory, _userSession, _notificationService);
        }
        public IEnumerable<KQLichSuDatVe> ApplyFilter(IEnumerable<KQLichSuDatVe> source)
        {
            var filter = source;

            if (!string.IsNullOrWhiteSpace(NoiDiFilter))
                filter = filter.Where(v => v.DiemDi == NoiDiFilter);

            if (!string.IsNullOrWhiteSpace(NoiDenFilter))
                filter = filter.Where(v => v.DiemDen == NoiDenFilter);

            if (!string.IsNullOrWhiteSpace(HangHangKhongFilter))
                filter = filter.Where(v => v.HangHangKhong == HangHangKhongFilter);

            if (NgayDatFilter.HasValue)
                filter = filter.Where(v => v.NgayDat?.Date == NgayDatFilter.Value.Date);

            if (!string.IsNullOrWhiteSpace(BookingStatusFilter) &&
                BookingStatusFilter != "Tất cả")
            {
                filter = filter.Where(v => v.TrangThai == BookingStatusFilter);
            }

            return filter;
        }

        [RelayCommand]
        private void SearchHistory()
        {
            if (rootHistoryBooking == null || rootHistoryBooking.Count == 0)
                return;

            var filtered = ApplyFilter(rootHistoryBooking);
            HistoryBooking = new ObservableCollection<KQLichSuDatVe>(filtered);
            IsEmpty = HistoryBooking.Count == 0;
        }

        public string? ValidateCancelTicket(KQLichSuDatVe ve)
        {
            if (ve == null) return "Vé không hợp lệ.";

            if (ve.CanCancel == false)
                return "Vé không thể hủy vì đã quá thời hạn hủy vé.";

            if (ve.TrangThai == "Đã hủy")
                return "Vé đã được hủy trước đó.";

            return null;
        }

        [RelayCommand]
        public async Task CancelTicket(KQLichSuDatVe ve)
        {
            var error = ValidateCancelTicket(ve);
            if (error != null)
            {
                await _notificationService.ShowNotificationAsync(error, NotificationType.Warning);
                return;
            }

            bool confirm = await _notificationService.ShowNotificationAsync(
                "Bạn có chắc chắn muốn hủy vé này không?",
                NotificationType.Information,
                isConfirmation: true);

            if (!confirm) return;

            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                var booking = await context.Datves
                    .Include(b => b.MaLbNavigation)
                    .FirstOrDefaultAsync(b => b.MaDv == ve.MaVe);

                if (booking == null)
                {
                    await _notificationService.ShowNotificationAsync(
                        "Không tìm thấy vé để hủy.",
                        NotificationType.Error);
                    return;
                }

                bool isPaid = booking.TtdatVe == "Đã thanh toán";
                string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;

                var chiTietVe = await context.Ctdvs
                    .Where(ct => ct.MaDv == ve.MaVe)
                    .ToListAsync();

                var maHvLb = chiTietVe.FirstOrDefault()?.MaHvLb;
                if (maHvLb != null)
                {
                    var hangVe = await context.Hangvetheolichbays
                        .FirstOrDefaultAsync(h => h.MaHvLb == maHvLb);

                    if (hangVe != null)
                        hangVe.SlveConLai += chiTietVe.Count;
                }

                booking.TtdatVe = "Đã hủy";
                await context.SaveChangesAsync();

                ve.TrangThai = "Đã hủy";
                OnPropertyChanged(nameof(HistoryBooking));

                await _notificationService.ShowNotificationAsync(
                    "Hủy vé thành công.",
                    NotificationType.Information);

                if (isPaid)
                {
                    var emailBody = _templateService.BuildBookingCancel(soHieuCb, gioDi, DateTime.Now);
                    await _emailService.SendEmailAsync(
                        booking.Email ?? _userSession.Email,
                        $"Huỷ vé chuyến bay {soHieuCb}",
                        emailBody);
                }

                await LoadData(_userSession.CustomerId);
            }
            catch (Exception ex)
            {
                await _notificationService.ShowNotificationAsync(
                    $"Lỗi khi hủy vé: {ex.Message}",
                    NotificationType.Error);
            }
        }

        [RelayCommand]
        public async Task ProcessPayment(KQLichSuDatVe ve)
        {
            try
            {
                PaymentDebugHelper.LogUserSession();
                PaymentDebugHelper.ValidateUserSessionForPayment();

                _userSession.CurrentTicketId = ve.MaVe.Value;
                string orderInfo = $"Thanhtoanvemaybay{ve.MaVe}";

                string paymentUrl = _paymentGateway.CreatePaymentUrl(
                    (double)ve.TongTienTT, orderInfo, ve.MaVe.Value);

                if (!string.IsNullOrEmpty(paymentUrl))
                {
                    WeakReferenceMessenger.Default.Send(new PaymentRequestedMessage(paymentUrl));
                }
                else
                {
                    await _notificationService.ShowNotificationAsync(
                        "Không thể tạo URL thanh toán VNPay",
                        NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowNotificationAsync(
                    $"Lỗi xử lý thanh toán VNPay: {ex.Message}",
                    NotificationType.Error);
            }
        }


        public void HandlePaymentSuccess()
        {
            try
            {
                PaymentDebugHelper.LogUserSession();
                PaymentDebugHelper.ValidateUserSessionForPayment();

                using var context = _dbContextFactory.CreateDbContext();

                var datVe = context.Datves
                    .Include(dv => dv.MaLbNavigation)
                    .Where(dv => dv.MaDv == _userSession.CurrentTicketId)
                    .OrderByDescending(dv => dv.ThoiGianDv)
                    .FirstOrDefault();

                if (datVe != null && datVe.TtdatVe == "Chưa thanh toán (Online)")
                {
                    datVe.TtdatVe = "Đã thanh toán";
                    context.SaveChanges();

                    string soHieuCb = datVe.MaLbNavigation?.SoHieuCb ?? "";
                    var emailContent = _templateService.BuildBookingSuccess(
                        soHieuCb,
                        datVe.MaLbNavigation?.GioDi ?? DateTime.Now,
                        DateTime.Now,
                        datVe.TongTienTt ?? 0);

                    _ = _emailService.SendEmailAsync(
                        datVe.Email ?? _userSession.Email,
                        $"Thanh toán vé chuyến bay {soHieuCb}",
                        emailContent);

                    _ = LoadData(_userSession.CustomerId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HandlePaymentSuccess] Lỗi cập nhật thanh toán: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ClearFilter()
        {
            NgayDatFilter = null;
            NoiDiFilter = null;
            NoiDenFilter = null;
            HangHangKhongFilter = null;
            HistoryBooking = new ObservableCollection<KQLichSuDatVe>(rootHistoryBooking);
            IsEmpty = HistoryBooking.Count == 0;
        }

        partial void OnSanBayListChanged(ObservableCollection<string> value)
        {
            OnPropertyChanged(nameof(DiemDiList));
            OnPropertyChanged(nameof(DiemDenList));
        }

        [ObservableProperty]
        private bool isSearchExpanded = true;

        [ObservableProperty]
        private double searchContentHeight = double.NaN;

        [RelayCommand]
        private void ToggleSearch()
        {
            IsSearchExpanded = !IsSearchExpanded;
        }
    }
}