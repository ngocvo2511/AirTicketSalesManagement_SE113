using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel.Admin;
using AirTicketSalesManagement.ViewModel.Customer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace AirTicketSalesManagement.ViewModel.Staff
{
    public partial class TicketManagementDetailViewModel : BaseViewModel
    {
        // Injected dependencies for testability
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService? _emailService;
        private readonly IEmailTemplateService? _templateService;
        private readonly BaseViewModel? parent;

        [ObservableProperty]
        private QuanLiDatVe chiTietVe;
        [ObservableProperty]
        private int tongTien;
        [ObservableProperty]
        private ObservableCollection<Ctdv>? ctdvList;
        [ObservableProperty]
        private bool canCancle;

        public NotificationViewModel Notification { get; }

        // Parameterless ctor kept for runtime/designer compatibility (uses concrete services)
        public TicketManagementDetailViewModel()
            : this(null, null, new AirTicketDbService(), new NotificationService(new NotificationViewModel()), null, null)
        {
        }

        // Backwards-compatible ctor (old app calls)
        public TicketManagementDetailViewModel(QuanLiDatVe chiTietVe, BaseViewModel parent)
            : this(chiTietVe, parent, new AirTicketDbService(), new NotificationService(new NotificationViewModel()), null, null)
        {
        }

        // Backwards-compatible ctor with email services
        public TicketManagementDetailViewModel(QuanLiDatVe chiTietVe, BaseViewModel parent, IEmailService emailService, EmailTemplateService templateService)
            : this(chiTietVe, parent, new AirTicketDbService(), new NotificationService(new NotificationViewModel()), emailService, templateService)
        {
        }

        // DI ctor - use in unit tests to inject fakes/mocks
        public TicketManagementDetailViewModel(
            QuanLiDatVe? chiTietVe,
            BaseViewModel? parent,
            IAirTicketDbContextService dbContextService,
            INotificationService notificationService,
            IEmailService? emailService,
            IEmailTemplateService? templateService)
        {
            _dbContextService = dbContextService ?? throw new ArgumentNullException(nameof(dbContextService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _emailService = emailService;
            _templateService = templateService;
            this.parent = parent;
            Notification = (notificationService as NotificationService)?.ViewModel
                       ?? new NotificationViewModel();

            if (chiTietVe != null)
            {
                ChiTietVe = chiTietVe;
                // load data is public async so tests can await it
                _ = LoadDataAsync();
            }
        }

        private AirTicketDbContext CreateContext() => _dbContextService.CreateDbContext();

        [ExcludeFromCodeCoverage]
        public async Task LoadDataAsync()
        {
            CanCancle = ChiTietVe?.CanCancel ?? false;

            try
            {
                using var context = CreateContext();
                var result = await context.Ctdvs
                    .Where(ctdv => ctdv.MaDv == ChiTietVe.MaVe)
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
            catch (Exception)
            {
                // Prefer injected notification service; fall back to UI VM
                await _notification_service_fallback("Lỗi khi tải dữ liệu hành khách.", NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        private void GoBack()
        {
            if (parent is StaffViewModel staffViewModel)
            {
                staffViewModel.CurrentViewModel = new TicketManagementViewModel(
                    parent,
                    _dbContextService,
                    _notificationService,
                    _emailService,
                    _templateService);
            }
            else if (parent is AdminViewModel adminViewModel)
            {
                adminViewModel.CurrentViewModel = new TicketManagementViewModel(
                    parent,
                    _dbContextService,
                    _notificationService,
                    _emailService,
                    _templateService);
            }
        }

        // made public Task so tests can await
        [RelayCommand]
        public async Task CancelTicket()
        {
            if (ChiTietVe == null) return;

            if (ChiTietVe.TrangThai == "Đã hủy")
            {
                await _notification_service_fallback("Vé đã được hủy trước đó.", NotificationType.Warning);
                return;
            }
            if (ChiTietVe.CanCancel == false)
            {
                await _notification_service_fallback("Vé không thể hủy do đã quá thời gian hủy.", NotificationType.Warning);
                return;
            }

            bool confirm = await _notification_service_fallback("Bạn có chắc chắn muốn hủy vé này không?", NotificationType.Information, true);
            if (!confirm) return;

            try
            {
                using var context = CreateContext();
                var booking = await context.Datves.Include(b => b.MaLbNavigation).FirstOrDefaultAsync(b => b.MaDv == ChiTietVe.MaVe);
                if (booking != null)
                {
                    bool isPaid = booking.TtdatVe == "Đã thanh toán";
                    string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                    DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;
                    var ctdvList = await context.Ctdvs.Where(ct => ct.MaDv == ChiTietVe.MaVe).ToListAsync();
                    var maHvLb = ctdvList.FirstOrDefault()?.MaHvLb;
                    if (maHvLb != null)
                    {
                        var hangVe = await context.Hangvetheolichbays.FirstOrDefaultAsync(h => h.MaHvLb == maHvLb);
                        if (hangVe != null)
                        {
                            hangVe.SlveConLai += ctdvList.Count;
                        }
                    }

                    booking.TtdatVe = "Đã hủy";
                    await context.SaveChangesAsync();

                    await _notification_service_fallback("Hủy vé thành công.", NotificationType.Information);

                    ChiTietVe.TrangThai = "Đã hủy";
                    OnPropertyChanged(nameof(ChiTietVe));

                    if (isPaid && _emailService != null && _templateService != null)
                    {
                        var emailBody = _templateService.BuildBookingCancel(soHieuCb, gioDi, DateTime.Now);
                        await _emailService.SendEmailAsync(booking.Email ?? UserSession.Current.Email, $"Huỷ vé chuyến bay {soHieuCb}", emailBody);
                    }
                }
                else
                {
                    await _notification_service_fallback("Không tìm thấy vé để hủy.", NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi hủy vé: {ex.Message}", NotificationType.Error);
            }
        }

        [RelayCommand]
        public async Task ConfirmPayment()
        {
            if (ChiTietVe == null) return;

            if (ChiTietVe.TrangThai != "Chờ thanh toán" && ChiTietVe.TrangThai != "Chưa thanh toán (Tiền mặt)")
            {
                await _notification_service_fallback("Không thể xác nhận thanh toán.", NotificationType.Warning);
                return;
            }
            if (ChiTietVe.CanConfirm == false)
            {
                await _notification_service_fallback("Vé không thể thanh toán do đã quá thời gian đặt vé.", NotificationType.Warning);
                return;
            }

            bool confirm = await _notification_service_fallback("Bạn có chắc chắn muốn xác nhận thanh toán vé này không?", NotificationType.Information, true);
            if (!confirm) return;

            try
            {
                using var context = CreateContext();
                var booking = await context.Datves.Include(b => b.MaLbNavigation).FirstOrDefaultAsync(b => b.MaDv == ChiTietVe.MaVe);
                if (booking != null)
                {
                    string soHieuCb = booking.MaLbNavigation?.SoHieuCb ?? "";
                    decimal price = booking.TongTienTt ?? 0;
                    DateTime gioDi = booking.MaLbNavigation?.GioDi ?? DateTime.Now;

                    booking.TtdatVe = "Đã thanh toán";
                    await context.SaveChangesAsync();

                    await _notification_service_fallback("Xác nhận thanh toán thành công.", NotificationType.Information);

                    ChiTietVe.TrangThai = "Đã thanh toán";
                    OnPropertyChanged(nameof(ChiTietVe));

                    if (_emailService != null && _templateService != null)
                    {
                        var emailBody = _templateService.BuildBookingSuccess(soHieuCb, gioDi, DateTime.Now, price);
                        await _emailService.SendEmailAsync(booking.Email ?? UserSession.Current.Email, $"Thanh toán vé chuyến bay {soHieuCb}", emailBody);
                    }
                }
                else
                {
                    await _notification_service_fallback("Không tìm thấy vé để xác nhận thanh toán.", NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi xác nhận thanh toán: {ex.Message}", NotificationType.Error);
            }
        }

        // prefer injected notification service; fallback to Notification VM if service fails
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