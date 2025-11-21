using AirTicketSalesManagement.Data;
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
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace AirTicketSalesManagement.ViewModel.Customer
{
    public partial class BookingHistoryDetailViewModel : BaseViewModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _templateService;
        private readonly CustomerViewModel _parent;
        private readonly IAirTicketDbContextService _dbContextFactory;
        private readonly IUserSessionService _userSession;
        private readonly INotificationService _notificationService;

        // ✔ property duy nhất cho Notification
        [ObservableProperty]
        private NotificationViewModel notification;

        [ObservableProperty]
        private KQLichSuDatVe lichSuDatVe;

        [ObservableProperty]
        private int tongTien;

        [ObservableProperty]
        private ObservableCollection<Ctdv>? ctdvList;

        [ObservableProperty]
        private bool canCancle;


        // Constructor dùng cho DI / runtime
        public BookingHistoryDetailViewModel(
            KQLichSuDatVe lichSuDatVe,
            CustomerViewModel parent,
            IEmailService emailService,
            EmailTemplateService templateService,
            IAirTicketDbContextService dbContextFactory,
            IUserSessionService userSession,
            NotificationViewModel notificationVm,  
            INotificationService notificationService)      
        {
            LichSuDatVe = lichSuDatVe;
            _parent = parent;
            _emailService = emailService;
            _templateService = templateService;
            _dbContextFactory = dbContextFactory;
            _userSession = userSession;
            _notificationService = notificationService;

            // gán đúng property auto-generated
            Notification = notificationVm;
        }

        // Constructor default (dùng cho designer hoặc test)
        public BookingHistoryDetailViewModel()
        {
            Notification = new NotificationViewModel();
        }

        public async Task LoadData()
        {
            CanCancle = LichSuDatVe.CanCancel;

            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                var result = await context.Ctdvs
                    .Where(ctdv => ctdv.MaDv == LichSuDatVe.MaVe)
                    .Select(ctdv => new Ctdv
                    {
                        MaDv = ctdv.MaDv,
                        MaCtdv = ctdv.MaCtdv,
                        HoTenHk = ctdv.HoTenHk,
                        GioiTinh = ctdv.GioiTinh,
                        NgaySinh = ctdv.NgaySinh,
                        Cccd = ctdv.Cccd,
                        HoTenNguoiGiamHo = ctdv.HoTenNguoiGiamHo,
                        MaHvLb = ctdv.MaHvLb,
                        GiaVeTt = ctdv.GiaVeTt
                    })
                    .ToListAsync();

                CtdvList = new ObservableCollection<Ctdv>(result);
            }
            catch
            {
                // optional: await _notificationService.ShowNotificationAsync("Lỗi tải dữ liệu", NotificationType.Error);
            }
        }

        public string? ValidateCancelTicket()
        {
            if (LichSuDatVe == null)
                return "Vé không hợp lệ.";

            if (LichSuDatVe.TrangThai == "Đã hủy")
                return "Vé đã được hủy trước đó.";

            if (!LichSuDatVe.CanCancel)
                return "Vé không thể hủy do đã quá thời gian hủy.";

            return null;
        }

        [RelayCommand]
        private void GoBack()
        {
            _parent.CurrentViewModel = new BookingHistoryViewModel(
                _userSession.CustomerId,
                _parent,
                _emailService,
                _templateService
                ,_dbContextFactory,
                new VnpayPaymentGateway(),
                _userSession,
                _notificationService);
        }

        [RelayCommand]
        private async Task CancelTicket()
        {
            var error = ValidateCancelTicket();
            if (error != null)
            {
                await _notificationService.ShowNotificationAsync(error, NotificationType.Warning);
                return;
            }

            bool confirmResult = await _notificationService.ShowNotificationAsync(
                "Bạn có chắc chắn muốn hủy vé này không?",
                NotificationType.Warning,
                isConfirmation: true);

            if (!confirmResult) return;

            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                var booking = await context.Datves
                    .Include(b => b.MaLbNavigation)
                    .FirstOrDefaultAsync(b => b.MaDv == LichSuDatVe.MaVe);

                if (booking == null)
                {
                    await _notificationService.ShowNotificationAsync("Không tìm thấy vé để hủy.", NotificationType.Error);
                    return;
                }

                bool isPaid = booking.TtdatVe == "Đã thanh toán";
                string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;

                var ctdvList = await context.Ctdvs
                    .Where(ctdv => ctdv.MaDv == LichSuDatVe.MaVe)
                    .ToListAsync();

                var maHvLb = ctdvList.FirstOrDefault()?.MaHvLb;

                if (maHvLb != null)
                {
                    var hangVe = await context.Hangvetheolichbays
                        .FirstOrDefaultAsync(h => h.MaHvLb == maHvLb);

                    if (hangVe != null)
                        hangVe.SlveConLai += ctdvList.Count;
                }

                booking.TtdatVe = "Đã hủy";
                await context.SaveChangesAsync();

                LichSuDatVe.TrangThai = "Đã hủy";
                OnPropertyChanged(nameof(LichSuDatVe));

                await _notificationService.ShowNotificationAsync("Hủy vé thành công.", NotificationType.Information);

                if (isPaid)
                {
                    var emailBody = _templateService.BuildBookingCancel(soHieuCb, gioDi, DateTime.Now);
                    await _emailService.SendEmailAsync(
                        booking.Email ?? _userSession.Email,
                        $"Huỷ vé chuyến bay {soHieuCb}",
                        emailBody);
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowNotificationAsync($"Lỗi khi hủy vé: {ex.Message}", NotificationType.Error);
            }
        }
    }
}
