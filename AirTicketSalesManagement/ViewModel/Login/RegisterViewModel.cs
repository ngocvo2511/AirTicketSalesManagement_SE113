using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Register;
using AirTicketSalesManagement.Services.Timer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

namespace AirTicketSalesManagement.ViewModel.Login
{
    public partial class RegisterViewModel : ValidationBase
    {
        private readonly IRegisterService _registerService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITimerService _timerService;
        private readonly IEmailValidation _emailValidation;
        private readonly AuthViewModel _auth;
        public ToastViewModel Toast { get; }

        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string name;
        [ObservableProperty] private bool isOtpStep;
        [ObservableProperty] private string otpCode;
        [ObservableProperty] private string? timeLeftText;

        public RegisterViewModel(AuthViewModel auth,
            IRegisterService registerService,
            IEmailService emailService,
            IOtpService otpService,
            IEmailTemplateService emailTemplateService,
            ITimerService timerService,
            IEmailValidation emailValidation,
            ToastViewModel toast)
        {
            _registerService = registerService;
            _emailService = emailService;
            _otpService = otpService;
            _emailTemplateService = emailTemplateService;
            _timerService = timerService;
            Toast = toast;
            _auth = auth;
            _emailValidation = emailValidation;
        }
        public override async Task ValidateAsync()
        {
            ClearErrors(nameof(Email));
            ClearErrors(nameof(Password));
            ClearErrors(nameof(ConfirmPassword));
            ClearErrors(nameof(Name));

            if (string.IsNullOrWhiteSpace(Name))
                AddError(nameof(Name), "Tên không được để trống.");
            else if (Name.Length > 30)
                AddError(nameof(Name), "Tên vượt quá giới hạn cho phép");

            if (string.IsNullOrWhiteSpace(Email))
                AddError(nameof(Email), "Email không được để trống.");
            else if (Email.Length > 254)
                AddError(nameof(Email), "Email vượt quá giới hạn cho phép");
            else if (!_emailValidation.IsValid(Email))
                AddError(nameof(Email), "Email không hợp lệ.");
            else if (await _registerService.IsEmailExistsAsync(Email))
                AddError(nameof(Email), "Email đã được đăng ký");

            if (string.IsNullOrWhiteSpace(Password))
                AddError(nameof(Password), "Mật khẩu không được để trống.");
            else if (Password.Length > 100)
                AddError(nameof(Password), "Mật khẩu vượt quá giới hạn cho phép");

            if (ConfirmPassword != Password)
                AddError(nameof(ConfirmPassword), "Xác nhận mật khẩu không khớp với mật khẩu.");
        }

        #region Commands
        [RelayCommand]
        private async Task Register()
        {
            await ValidateAsync();
            if (HasErrors) return;

            IsOtpStep = true;
            string otp = _otpService.GenerateOtp(Email);
            string emailContent = _emailTemplateService.BuildRegisterOtp(otp);

            try
            {
                _timerService.Start(TimeSpan.FromMinutes(3),
                    onTick: t => TimeLeftText = $"Mã hết hạn sau: {t.Minutes:D2}:{t.Seconds:D2}",
                    onFinished: () => TimeLeftText = "Mã xác nhận đã hết hạn."
                );

                await _emailService.SendEmailAsync(Email, "Yêu cầu đăng kí tài khoản", emailContent);
                await Toast.ShowToastAsync("Mã xác nhận đã được gửi đến email của bạn.", Brushes.Green);
            }
            catch
            {
                await Toast.ShowToastAsync("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", Brushes.OrangeRed);
            }
        }

        [RelayCommand]
        private async Task ConfirmOtp()
        {
            ClearErrors(nameof(OtpCode));
            if (string.IsNullOrWhiteSpace(OtpCode))
            {
                AddError(nameof(OtpCode), "Mã OTP không được để trống.");
                return;
            }

            if (_otpService.VerifyOtp(Email, OtpCode))
            {
                bool success = await _registerService.CreateCustomerAsync(Name, Email, Password);
                if (!success)
                {
                    await Toast.ShowToastAsync("Không thể kết nối đến cơ sở dữ liệu", Brushes.OrangeRed);
                    return;
                }

                await Toast.ShowToastAsync("Đăng kí thành công. Vui lòng đăng nhập.", Brushes.Green);
                _auth.NavigateToLogin();
            }
            else
            {
                AddError(nameof(OtpCode), "Mã OTP không hợp lệ hoặc đã hết hạn.");
            }
        }

        [RelayCommand]
        private async Task ResendOtp()
        {
            string otp = _otpService.GenerateOtp(Email);
            string emailContent = _emailTemplateService.BuildRegisterOtp(otp);

            try
            {
                await _emailService.SendEmailAsync(Email, "Yêu cầu đặt lại mật khẩu", emailContent);
                await Toast.ShowToastAsync("Mã xác nhận đã được gửi đến email của bạn.", Brushes.Green);
                _timerService.Stop();
                _timerService.Start(TimeSpan.FromMinutes(3),
                    onTick: t => TimeLeftText = $"Mã hết hạn sau: {t.Minutes:D2}:{t.Seconds:D2}",
                    onFinished: () => TimeLeftText = "Mã xác nhận đã hết hạn."
                );
            }
            catch
            {
                await Toast.ShowToastAsync("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", Brushes.OrangeRed);
            }
        }
        #endregion
        [RelayCommand]
        private void ShowLogin() => _auth.NavigateToLogin();
    }
}
