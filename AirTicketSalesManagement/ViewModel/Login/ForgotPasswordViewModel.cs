using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.ForgotPassword;
using AirTicketSalesManagement.Services.ResetPassword;
using AirTicketSalesManagement.Services.Timer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AirTicketSalesManagement.ViewModel.Login
{
    public partial class ForgotPasswordViewModel : ValidationBase
    {
        private readonly AuthViewModel _auth;
        public ToastViewModel Toast { get; }
        private readonly IForgotPasswordService _forgotPasswordService;
        [ObservableProperty]
        private string email;

        public ForgotPasswordViewModel(){}

        public ForgotPasswordViewModel(AuthViewModel auth, IForgotPasswordService forgotPasswordService, ToastViewModel toast)
        {
            _auth = auth;
            Toast = toast;
            _forgotPasswordService = forgotPasswordService;
        }

        public override async Task<bool> ValidateAsync()
        {
            ClearErrors(nameof(Email));

            if (string.IsNullOrWhiteSpace(Email))
            {
                AddError(nameof(Email), "Email không được để trống.");
                return false;
            }
            if (!_forgotPasswordService.IsValid(Email))
            {
                AddError(nameof(Email), "Email không hợp lệ.");
                return false;
            }

            bool exists;

            try
            {
                exists = await _forgotPasswordService.EmailExistsAsync(Email);
            }
            catch
            {
                await Toast.ShowToastAsync("Lỗi kết nối cơ sở dữ liệu.");
                return false;
            }

            if (!exists)
            {
                AddError(nameof(Email), "Tài khoản không tồn tại.");
                return false;
            }

            return true;
        }
        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            if (!await ValidateAsync())
                return;
            _auth.CurrentViewModel = new ResetPasswordViewModel(_auth, Email, new ResetPasswordService(new AirTicketDbContext()), new EmailService(), new OtpService(), new EmailTemplateService(), new DispatcherTimerService(), new ToastViewModel());
        }

        [RelayCommand]
        private void ShowLogin() => _auth.NavigateToLogin();
    }
}
