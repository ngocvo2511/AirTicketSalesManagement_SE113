using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace AirTicketSalesManagement.ViewModel.Staff
{
    public partial class TicketManagementViewModel : BaseViewModel
    {
        // --- Injected dependencies (for testability) ---
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;

        private readonly BaseViewModel parent;
        private ObservableCollection<QuanLiDatVe> rootHistoryBooking = new();

        [ObservableProperty]
        private DateTime? ngayDatFilter;
        [ObservableProperty]
        private string noiDiFilter;
        [ObservableProperty]
        private string noiDenFilter;
        [ObservableProperty]
        private string? hangHangKhongFilter;
        [ObservableProperty]
        private string emailFilter;
        [ObservableProperty]
        private string bookingStatusFilter;
        [ObservableProperty]
        private ObservableCollection<string> bookingStatusList = new();
        [ObservableProperty]
        private ObservableCollection<string> hangHangKhongList = new();
        [ObservableProperty]
        private ObservableCollection<string> sanBayList = new();

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
        private ObservableCollection<QuanLiDatVe>? historyBooking = new();
        [ObservableProperty]
        private bool isEmpty;
        public NotificationViewModel Notification { get; }
        // Parameterless ctor - keep behavior for designer/runtime (no DB actions)
        public TicketManagementViewModel()
        {
            _dbContextService = new AirTicketDbService();
            _notificationService = new NotificationService(new NotificationViewModel());
        }

        // Existing ctor kept for runtime usage when app provides email services
        public TicketManagementViewModel(BaseViewModel parent)
            : this(parent, new AirTicketDbService(), new NotificationService(new NotificationViewModel()), null, null)
        {
        }

        public TicketManagementViewModel(BaseViewModel parent, IEmailService emailService, IEmailTemplateService templateService)
            : this(parent, new AirTicketDbService(), new NotificationService(new NotificationViewModel()), emailService, templateService)
        {
        }

        // DI ctor - use in unit tests to inject fakes/mocks
        public TicketManagementViewModel(BaseViewModel parent, IAirTicketDbContextService dbContextService, INotificationService notificationService, IEmailService? emailService, IEmailTemplateService? templateService)
        {
            _dbContextService = dbContextService ?? throw new System.ArgumentNullException(nameof(dbContextService));
            _notificationService = notificationService ?? throw new System.ArgumentNullException(nameof(notificationService));
            _emailService = emailService!;
            Notification = (notificationService as NotificationService)?.ViewModel
                       ?? new NotificationViewModel();
            this.parent = parent;

            // fire-and-forget initial load (tests can call LoadData explicitly)
            _ = LoadData();
            ClearExpiredHolds();
        }

        // Create DbContext via injected factory
        private AirTicketDbContext CreateContext() => _dbContextService.CreateDbContext();

        // Public so tests can await
        public async Task LoadData()
        {
            try
            {
                using var context = CreateContext();
                var quiDinh = await context.Quydinhs.FirstOrDefaultAsync();
                var query = from datve in context.Datves
                            join lichbay in context.Lichbays on datve.MaLb equals lichbay.MaLb
                            join chuyenbay in context.Chuyenbays on lichbay.SoHieuCb equals chuyenbay.SoHieuCb
                            join sbDi in context.Sanbays on chuyenbay.Sbdi equals sbDi.MaSb
                            join sbDen in context.Sanbays on chuyenbay.Sbden equals sbDen.MaSb
                            join khachhang in context.Khachhangs on datve.MaKh equals khachhang.MaKh into khGroup
                            from kh in khGroup.DefaultIfEmpty()
                            join taikhoanKH in context.Taikhoans on kh.MaKh equals taikhoanKH.MaKh into tkKhGroup
                            from tkKh in tkKhGroup.DefaultIfEmpty()
                            join nhanvien in context.Nhanviens on datve.MaNv equals nhanvien.MaNv into nvGroup
                            from nv in nvGroup.DefaultIfEmpty()
                            join taikhoanNV in context.Taikhoans on nv.MaNv equals taikhoanNV.MaNv into tkNvGroup
                            from tkNv in tkNvGroup.DefaultIfEmpty()
                            select new QuanLiDatVe
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
                                HoTenNguoiDat = kh != null ? kh.HoTenKh : (nv != null ? nv.HoTenNv : "Không rõ"),
                                EmailNguoiDat = tkKh != null ? tkKh.Email : (tkNv != null ? tkNv.Email : ""),
                                NgayDat = datve.ThoiGianDv,
                                TrangThai = datve.TtdatVe,
                                SoLuongKhach = datve.Ctdvs.Count,
                                QdDatVe = (quiDinh != null) ? quiDinh.TgdatVeChamNhat : null,
                                QdHuyVe = (quiDinh != null) ? quiDinh.TghuyDatVe : null
                            };
                var result = await query.OrderByDescending(x => x.NgayDat).ToListAsync();
                rootHistoryBooking = new ObservableCollection<QuanLiDatVe>(result);
                HistoryBooking = new ObservableCollection<QuanLiDatVe>(result);
                IsEmpty = HistoryBooking.Count == 0;
                var airlineName = await context.Chuyenbays
                                .Select(v => v.HangHangKhong)
                                .Distinct()
                                .ToListAsync();
                HangHangKhongList = new ObservableCollection<string>([.. airlineName]);
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
                    "Giữ chỗ",
                    "Đã hủy"
                };
                BookingStatusFilter = "Tất cả";
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Lỗi kết nối cơ sở dữ liệu", e);
            }
        }

        // Keep synchronous compatibility; operates via injected CreateContext
        public void ClearExpiredHolds()
        {
            try
            {
                using var context = CreateContext();
                var quiDinh = context.Quydinhs.FirstOrDefault();
                int tgHuy = quiDinh?.TghuyDatVe ?? 0;
                var expiredDatVes = context.Datves
                    .Where(dv => (dv.TtdatVe == "Chưa thanh toán (Online)" || dv.TtdatVe == "Giữ chỗ") &&
                                 (dv.ThoiGianDv < DateTime.Now.AddMinutes(-20) || (dv.MaLbNavigation != null && dv.MaLbNavigation.GioDi.HasValue && DateTime.Now.Date >= dv.MaLbNavigation.GioDi.Value.AddDays(-tgHuy).Date)))
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
                    && dv.MaLbNavigation != null && dv.MaLbNavigation.GioDi.HasValue
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
            catch (System.Exception)
            {
                // swallow - keep old behavior
            }
        }

        [RelayCommand]
        private void ShowDetailHistory(QuanLiDatVe chiTietVe)
        {
            if (parent is StaffViewModel staffViewModel)
            {
                staffViewModel.CurrentViewModel = new TicketManagementDetailViewModel(chiTietVe, parent,_dbContextService, _notificationService, _emailService, _templateService);
            }
            else if (parent is AdminViewModel adminViewModel)
            {
                adminViewModel.CurrentViewModel = new TicketManagementDetailViewModel(chiTietVe, parent, _dbContextService, _notificationService, _emailService, _templateService);
            }
        }

        [RelayCommand]
        private void SearchHistory()
        {
            if (rootHistoryBooking == null) return;
            if (rootHistoryBooking.Count != 0)
            {
                var filter = rootHistoryBooking.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(NoiDiFilter))
                {
                    filter = filter.Where(v => v.DiemDi == NoiDiFilter);
                }
                if (!string.IsNullOrWhiteSpace(NoiDenFilter))
                {
                    filter = filter.Where(v => v.DiemDen == NoiDenFilter);
                }
                if (!string.IsNullOrWhiteSpace(HangHangKhongFilter))
                {
                    filter = filter.Where(v => v.HangHangKhong == HangHangKhongFilter);
                }
                if (NgayDatFilter.HasValue)
                {
                    filter = filter.Where(v => v.NgayDat?.Date == NgayDatFilter.Value.Date);
                }
                if (!string.IsNullOrWhiteSpace(BookingStatusFilter))
                {
                    filter = filter.Where(v => v.TrangThai == BookingStatusFilter || BookingStatusFilter == "Tất cả");
                }
                if (!string.IsNullOrWhiteSpace(EmailFilter))
                {
                    filter = filter.Where(v => v.EmailNguoiDat != null && v.EmailNguoiDat.Contains(EmailFilter, System.StringComparison.OrdinalIgnoreCase));
                }
                HistoryBooking = new ObservableCollection<QuanLiDatVe>(filter);
                IsEmpty = HistoryBooking.Count == 0;
            }
        }

        // made public Task so tests can await
        [RelayCommand]
        public async Task CancelTicket(QuanLiDatVe ve)
        {
            if (ve == null) return;
            if (ve.CanCancel == false)
            {
                await _notification_service_fallback("Không thể hủy vé này do đã quá thời hạn hủy.", NotificationType.Warning);
                return;
            }
            if (ve.TrangThai == "Đã hủy")
            {
                await _notification_service_fallback("Vé đã được hủy trước đó.", NotificationType.Warning);
                return;
            }
            bool confirm = await _notification_service_fallback("Bạn có chắc chắn muốn hủy vé này không?", NotificationType.Information, true);
            if (confirm)
            {
                try
                {
                    using var context = CreateContext();
                    var booking = await context.Datves.FirstOrDefaultAsync(b => b.MaDv == ve.MaVe);
                    if (booking != null)
                    {
                        bool isPaid = booking.TtdatVe == "Đã thanh toán";
                        string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                        DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;
                        var chiTietVe = await context.Ctdvs.Where(ct => ct.MaDv == ve.MaVe).ToListAsync();
                        var maHvLb = chiTietVe.FirstOrDefault()?.MaHvLb;
                        if (maHvLb != null)
                        {
                            var hangVe = await context.Hangvetheolichbays
                                .FirstOrDefaultAsync(h => h.MaHvLb == maHvLb);
                            if (hangVe != null)
                            {
                                hangVe.SlveConLai += chiTietVe.Count;
                            }
                        }
                        booking.TtdatVe = "Đã hủy";
                        await context.SaveChangesAsync();
                        ve.TrangThai = "Đã hủy";
                        OnPropertyChanged(nameof(HistoryBooking));
                        await _notification_service_fallback("Hủy vé thành công.", NotificationType.Information);
                        if (isPaid && _emailService != null && _templateService != null)
                        {
                            var emailBody = _templateService.BuildBookingCancel(soHieuCb, gioDi, System.DateTime.Now);
                            await _emailService.SendEmailAsync(booking.Email ?? UserSession.Current.Email, $"Huỷ vé chuyến bay {soHieuCb}", emailBody);
                        }
                        await LoadData();
                    }
                    else
                    {
                        await _notification_service_fallback("Không tìm thấy vé để hủy.", NotificationType.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    await _notification_service_fallback($"Lỗi khi hủy vé: {ex.Message}", NotificationType.Error);
                }
            }
        }

        [RelayCommand]
        public async Task ConfirmPayment(QuanLiDatVe ve)
        {
            if (ve == null) return;
            if (ve.CanConfirm == false)
            {
                await _notification_service_fallback("Không thể xác nhận thanh toán vé này do đã quá thời hạn đặt vé.", NotificationType.Warning);
                return;
            }
            if (ve.TrangThai != "Chưa thanh toán (Tiền mặt)")
            {
                await _notification_service_fallback("Không thể xác nhận thanh toán.", NotificationType.Warning);
                return;
            }
            bool confirm = await _notification_service_fallback("Bạn có chắc chắn muốn xác nhận thanh toán vé này không?", NotificationType.Information, true);
            if (confirm)
            {
                try
                {
                    using var context = CreateContext();
                    var booking = await context.Datves.Include(b => b.MaLbNavigation).FirstOrDefaultAsync(b => b.MaDv == ve.MaVe);
                    if (booking != null)
                    {
                        string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                        DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;
                        decimal price = booking.TongTienTt ?? 0;
                        booking.TtdatVe = "Đã thanh toán";
                        await context.SaveChangesAsync();
                        ve.TrangThai = "Đã thanh toán";
                        OnPropertyChanged(nameof(HistoryBooking));
                        await _notification_service_fallback("Xác nhận thanh toán thành công.", NotificationType.Information);
                        if (_emailService != null && _templateService != null)
                        {
                            var emailBody = _templateService.BuildBookingSuccess(soHieuCb, gioDi, System.DateTime.Now, price);
                            await _emailService.SendEmailAsync(booking.Email ?? UserSession.Current.Email, $"Thanh toán vé chuyến bay {soHieuCb}", emailBody);
                        }
                        await LoadData();
                    }
                    else
                    {
                        await _notification_service_fallback("Không tìm thấy vé để xác nhận thanh toán.", NotificationType.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    await _notification_service_fallback($"Lỗi khi xác nhận thanh toán: {ex.Message}", NotificationType.Error);
                }
            }
        }

        [RelayCommand]
        public void ClearFilter()
        {
            NgayDatFilter = null;
            NoiDiFilter = string.Empty;
            NoiDenFilter = string.Empty;
            HangHangKhongFilter = null;
            EmailFilter = string.Empty;
            BookingStatusFilter = "Tất cả";
            HistoryBooking = new ObservableCollection<QuanLiDatVe>(rootHistoryBooking);
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

        // prefer injected notification service; fallback to UI VM if service fails
        private async Task<bool> _notification_service_fallback(string message, NotificationType type, bool isConfirmation = false)
        {
            try
            {
                return await _notificationService.ShowNotificationAsync(message, type, isConfirmation);
            }
            catch
            {
                return await Notification.ShowNotificationAsync(message, type, isConfirmation);
            }
        }
    }
}