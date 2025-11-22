using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AirTicketSalesManagement.ViewModel.Customer
{
    public partial class CustomerProfileViewModel : BaseViewModel
    {
        // injected dependencies for testability
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string hoTen;
        [ObservableProperty]
        private string soDienThoai;
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string maKhachHang;
        [ObservableProperty]
        private string canCuoc;
        [ObservableProperty]
        private string gioiTinh;
        [ObservableProperty]
        private DateTime? ngaySinh;
        [ObservableProperty]
        private bool isEditPopupOpen;
        [ObservableProperty]
        private bool isChangePasswordPopupOpen;
        [ObservableProperty]
        private bool isPasswordVisible;

        [ObservableProperty]
        private string editHoTen;
        [ObservableProperty]
        private string editSoDienThoai;
        [ObservableProperty]
        private string editEmail;
        [ObservableProperty]
        private string editCanCuoc;
        [ObservableProperty]
        private string editGioiTinh;
        [ObservableProperty]
        private DateTime? editNgaySinh;

        [ObservableProperty]
        private string currentPassword;
        [ObservableProperty]
        private string newPassword;
        [ObservableProperty]
        private string confirmPassword;
        [ObservableProperty]
        private bool hasPasswordError = false;
        [ObservableProperty]
        private string passwordErrorMessage;

        // Notification (kept for UI binding fallback)
        public NotificationViewModel Notification { get; }

        // Parameterless ctor for runtime - uses concrete services
        public CustomerProfileViewModel()
            : this(new AirTicketDbService(), new NotificationService(new NotificationViewModel()))
        {
        }

        // DI ctor for unit tests (inject IAirTicketDbContextService, INotificationService)
        public CustomerProfileViewModel(IAirTicketDbContextService dbContextService, INotificationService notificationService)
        {
            _dbContextService = dbContextService ?? throw new ArgumentNullException(nameof(dbContextService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            // keep UI-bound Notification VM for compatibility
            Notification = (notificationService as NotificationService)?.ViewModel
                       ?? new NotificationViewModel();

            // load data asynchronously (tests can call LoadDataAsync explicitly)
            _ = LoadDataAsync();
        }

        private AirTicketDbContext CreateContext() => _dbContextService.CreateDbContext();

        // Public async variant so tests can await it
        public async Task LoadDataAsync()
        {
            try
            {
                using var context = CreateContext();
                var khachHang = await context.Khachhangs
                    .Include(kh => kh.Taikhoans)
                    .FirstOrDefaultAsync(kh => kh.MaKh == UserSession.Current.CustomerId);

                if (khachHang != null)
                {
                    HoTen = khachHang.HoTenKh;
                    SoDienThoai = khachHang.SoDt;
                    Email = khachHang.Taikhoans.FirstOrDefault()?.Email;
                    MaKhachHang = khachHang.MaKh.ToString();
                    CanCuoc = khachHang.Cccd;
                    GioiTinh = khachHang.GioiTinh;
                    NgaySinh = khachHang.NgaySinh.HasValue
                        ? khachHang.NgaySinh.Value.ToDateTime(TimeOnly.MinValue)
                        : (DateTime?)null;
                }
            }
            catch (Exception ex)
            {
                // prefer injected notification; fall back to UI VM
                await _notification_service_fallback($"Lỗi khi tải dữ liệu: {ex.Message}", NotificationType.Error);
            }
        }

        [RelayCommand]
        private void OpenEditProfile()
        {
            ResetField();
            IsEditPopupOpen = true;
        }

        private void ResetField()
        {
            EditHoTen = HoTen;
            EditSoDienThoai = SoDienThoai;
            EditCanCuoc = CanCuoc;
            EditGioiTinh = GioiTinh;
            EditNgaySinh = NgaySinh;
            EditEmail = Email;
        }

        [RelayCommand]
        private void CloseEditPopup()
        {
            IsEditPopupOpen = false;
        }

        // made public Task for testability
        [RelayCommand]
        public async Task SaveProfile()
        {
            try
            {
                using var context = CreateContext();
                var khachhang = await context.Khachhangs
                    .Include(nv => nv.Taikhoans)
                    .FirstOrDefaultAsync(kh => kh.MaKh == UserSession.Current.CustomerId);

                if (khachhang == null)
                {
                    await _notification_service_fallback("Không tìm thấy thông tin khách hàng.", NotificationType.Error);
                    return;
                }

                // Họ tên: bắt buộc phải nhập
                if (string.IsNullOrWhiteSpace(EditHoTen))
                {
                    await _notification_service_fallback("Họ tên không được để trống!", NotificationType.Warning);
                    EditHoTen = HoTen;
                    return;
                }
                khachhang.HoTenKh = EditHoTen;

                // Email: nếu có nhập thì phải đúng định dạng
                if (string.IsNullOrWhiteSpace(EditEmail))
                {
                    await _notification_service_fallback("Email không được để trống!", NotificationType.Warning);
                    EditEmail = Email;
                    return;
                }

                if (!IsValidEmail(EditEmail))
                {
                    await _notification_service_fallback("Email không hợp lệ!", NotificationType.Warning);
                    EditEmail = Email;
                    return;
                }

                bool emailExists = await context.Taikhoans
                    .AnyAsync(tk => tk.Email == EditEmail && tk.MaKh != khachhang.MaKh);

                if (emailExists)
                {
                    await _notification_service_fallback("Email đã được sử dụng bởi tài khoản khác!", NotificationType.Warning);
                    EditEmail = Email;
                    return;
                }

                var account = khachhang.Taikhoans.FirstOrDefault();
                if (account != null)
                    account.Email = EditEmail;

                // Số điện thoại: nếu có nhập thì kiểm tra định dạng
                if (!string.IsNullOrWhiteSpace(EditSoDienThoai))
                {
                    if (!IsValidPhone(EditSoDienThoai))
                    {
                        await _notification_service_fallback("Số điện thoại không hợp lệ!", NotificationType.Warning);
                        EditSoDienThoai = SoDienThoai;
                        return;
                    }
                    khachhang.SoDt = EditSoDienThoai;
                }
                else
                {
                    khachhang.SoDt = null;
                }

                // Căn cước: nếu có nhập thì kiểm tra độ dài hợp lệ (ví dụ 12 số)
                if (!string.IsNullOrWhiteSpace(EditCanCuoc))
                {
                    if (EditCanCuoc.Length != 12 || !EditCanCuoc.All(char.IsDigit))
                    {
                        await _notification_service_fallback("Số căn cước không hợp lệ!", NotificationType.Warning);
                        EditCanCuoc = CanCuoc;
                        return;
                    }
                    khachhang.Cccd = EditCanCuoc;
                }
                else
                {
                    khachhang.Cccd = null;
                }

                // Giới tính: nếu có nhập thì lưu
                if (!string.IsNullOrWhiteSpace(EditGioiTinh))
                {
                    khachhang.GioiTinh = EditGioiTinh;
                }

                // Ngày sinh: nếu có nhập thì phải nhỏ hơn ngày hiện tại
                if (EditNgaySinh.HasValue)
                {
                    if (EditNgaySinh.Value.Date >= DateTime.Today)
                    {
                        await _notification_service_fallback("Ngày sinh không hợp lệ!", NotificationType.Warning);
                        EditNgaySinh = NgaySinh;
                        return;
                    }
                    khachhang.NgaySinh = DateOnly.FromDateTime(EditNgaySinh.Value);
                }
                else
                {
                    khachhang.NgaySinh = null;
                }

                await context.SaveChangesAsync();
                await _notification_service_fallback("Cập nhật thông tin thành công!", NotificationType.Information);
                await LoadDataAsync();
                IsEditPopupOpen = false;
            }
            catch (Exception ex)
            {
                await _notification_service_fallback($"Đã xảy ra lỗi: {ex.Message}", NotificationType.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone ?? string.Empty, @"^0\d{9}$");
        }

        [RelayCommand]
        private void OpenChangePassword()
        {
            ResetPasswordField();
            IsChangePasswordPopupOpen = true;
        }

        [RelayCommand]
        private void CloseChangePasswordPopup()
        {
            IsChangePasswordPopupOpen = false;
        }

        [RelayCommand]
        public async Task ChangePassword()
        {
            HideError();
            try
            {
                using var context = CreateContext();
                var account = await context.Taikhoans.FirstOrDefaultAsync(tk => tk.MaKh == UserSession.Current.CustomerId);
                if (account == null)
                {
                    await _notification_service_fallback("Không tìm thấy tài khoản.", NotificationType.Error);
                    return;
                }

                // Kiểm tra mật khẩu hiện tại
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, account.MatKhau))
                {
                    ShowError("Mật khẩu hiện tại không đúng.");
                    return;
                }

                // Kiểm tra xác nhận mật khẩu mới
                if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword != ConfirmPassword)
                {
                    await _notification_service_fallback("Mật khẩu mới không khớp hoặc trống.", NotificationType.Warning);
                    return;
                }

                // Mã hóa mật khẩu mới
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                account.MatKhau = hashedPassword;

                await context.SaveChangesAsync();

                await _notification_service_fallback("Đổi mật khẩu thành công.", NotificationType.Information);

                // Xóa các trường để tránh lộ mật khẩu
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                IsChangePasswordPopupOpen = false;
            }
            catch (Exception ex)
            {
                await _notification_service_fallback($"Có lỗi xảy ra khi đổi mật khẩu: {ex.Message}", NotificationType.Error);
            }
        }

        private void ResetPasswordField()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            HasPasswordError = false;
            PasswordErrorMessage = string.Empty;
        }

        private void ShowError(string Error)
        {
            PasswordErrorMessage = Error;
            HasPasswordError = true;
        }

        private void HideError()
        {
            HasPasswordError = false;
            PasswordErrorMessage = string.Empty;
        }

        // prefer injected notification service; fallback to UI VM if service throws
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