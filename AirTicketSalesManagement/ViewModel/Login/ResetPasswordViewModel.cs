using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Login;
using AirTicketSalesManagement.Services.Navigation;
using AirTicketSalesManagement.Services.ResetPassword;
using AirTicketSalesManagement.Services.Timer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace AirTicketSalesManagement.ViewModel.Login
{
    public partial class ResetPasswordViewModel : ValidationBase
    {
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITimerService _timerService;
        private readonly IResetPasswordService _resetPasswordService;
        private AuthViewModel _auth;
        private ToastViewModel _toast { get; }
        [ObservableProperty]
        private string? code;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private bool isCodeValid;

        [ObservableProperty]
        private bool isCodeExpired;

        [ObservableProperty]
        private string? timeLeftText;

        [ObservableProperty]
        private bool canResendCode;
        public string? Email { get; set; }

        public ResetPasswordViewModel(AuthViewModel _auth, string Email, IResetPasswordService resetPasswordService, IEmailService emailService, IOtpService otpService, IEmailTemplateService emailTemplateService, ITimerService timerService, ToastViewModel Toast)
        {
            _toast = Toast;
            _resetPasswordService = resetPasswordService;
            _emailService = emailService;
            _otpService = otpService;
            _emailTemplateService = emailTemplateService;
            _timerService = timerService;
            _timerService.Start(TimeSpan.FromMinutes(3),
                   onTick: t => TimeLeftText = $"Mã hết hạn sau: {t.Minutes:D2}:{t.Seconds:D2}",
                   onFinished: () => TimeLeftText = "Mã xác nhận đã hết hạn."
               );
            this._auth = _auth;
            this.Email = Email;
            string otp = _otpService.GenerateOtp(Email);
            var emailContent = _emailTemplateService.BuildForgotPasswordOtp(otp);
            _emailService.SendEmailAsync(Email, "Yêu cầu đặt lại mật khẩu", emailContent).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _ = _toast.ShowToastAsync("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", Brushes.OrangeRed);
                }
                else
                {
                    _ = _toast.ShowToastAsync("Mã xác nhận đã được gửi đến email của bạn.", Brushes.Green);
                }
            });
        }
        [RelayCommand]

        public Task CheckCode()
        {
            ClearErrors(nameof(Code));
            if (IsCodeExpired)
            {
                AddError(nameof(Code), "Mã xác nhận đã hết hạn. Vui lòng gửi lại mã mới.");
                return Task.CompletedTask;
            }



            if (string.IsNullOrWhiteSpace(Code))
            {
                AddError(nameof(Code), "Mã xác nhận không được để trống.");
                return Task.CompletedTask;
            }

            bool isValid = _otpService.VerifyOtp(Email, Code);
            if (isValid)
            {
                IsCodeValid = true;
            }
            else
            {
                AddError(nameof(Code), "Mã xác nhận không hợp lệ hoặc đã hết hạn.");
                IsCodeValid = false;
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ResendCodeAsync()
        {
            string otp = _otpService.GenerateOtp(Email);
            var emailContent = _emailTemplateService.BuildForgotPasswordOtp(otp);
            await _emailService.SendEmailAsync(Email, "Yêu cầu đặt lại mật khẩu", emailContent);
            _timerService.Stop();
            _timerService.Start(TimeSpan.FromMinutes(3),
                onTick: t => TimeLeftText = $"Mã hết hạn sau: {t.Minutes:D2}:{t.Seconds:D2}",
                onFinished: () => TimeLeftText = "Mã xác nhận đã hết hạn."
            );
        }


        [RelayCommand]
        public async Task ResetPasswordAsync()
        {
            if (!IsCodeValid)
            {
                AddError(nameof(Code), "Vui lòng xác nhận mã trước khi đặt lại mật khẩu.");
                return;
            }
            await ValidateAsync();
            if (HasErrors) return;
            try
            {
                await _resetPasswordService.UpdatePasswordAsync(Email!, Password);

                _auth.CurrentViewModel = new LoginViewModel(_auth, new LoginService(new Data.AirTicketDbContext()), new NavigationWindowService(), new EmailValidation(), new ToastViewModel());
            }
            catch
            {
                await _toast.ShowToastAsync("Không thể kết nối đến cơ sở dữ liệu", Brushes.OrangeRed);
                return;
            }

        }
        public override Task ValidateAsync()
        {
            ClearErrors(nameof(Password));
            ClearErrors(nameof(ConfirmPassword));

            if (string.IsNullOrWhiteSpace(Password))
            {
                AddError(nameof(Password), "Mật khẩu không được để trống.");
            }
            else if (Password.Length > 100)
                AddError(nameof(Password), "Mật khẩu vượt quá giới hạn cho phép");
            if (ConfirmPassword != Password)
            {
                AddError(nameof(ConfirmPassword), "Xác nhận mật khẩu không khớp với mật khẩu.");
            }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void ShowLogin() => _auth.NavigateToLogin();
    }
}
