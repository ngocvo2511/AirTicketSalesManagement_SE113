using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Login;
using AirTicketSalesManagement.Services.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

namespace AirTicketSalesManagement.ViewModel.Login
{
    public partial class LoginViewModel : ValidationBase
    {
        private readonly AuthViewModel _auth;
        private readonly ILoginService _loginService;
        private readonly INavigationService _navigationService;
        private readonly IEmailValidation _emailValidation;
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPasswordVisible;

        public ToastViewModel Toast { get; }

        public LoginViewModel() { }

        public LoginViewModel(AuthViewModel auth, ILoginService loginService, INavigationService navigation, IEmailValidation emailValidation, ToastViewModel toast)
        {
            _auth = auth;
            _loginService = loginService;
            _navigationService = navigation;
            _emailValidation = emailValidation;
            Toast = toast;
        }

        public override Task ValidateAsync()
        {
            ClearErrors(nameof(Email));

            if (string.IsNullOrWhiteSpace(Email) ||
                !_emailValidation.IsValid(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                AddError(nameof(Email), "Tài khoản hoặc mật khẩu không hợp lệ");
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task Login()
        {
            await ValidateAsync();
            if (HasErrors) return;

            try
            {
                var result = await _loginService.LoginAsync(Email, Password);

                if (!result.Success)
                {
                    AddError(nameof(Email), result.Error);
                    return;
                }

                UpdateSession(result);
                Navigate(result.Role);
            }
            catch (Exception ex)
            {
                await Toast.ShowToastAsync(
                    "Không thể kết nối đến cơ sở dữ liệu.",
                    Brushes.OrangeRed
                );
            }
        }

        private void UpdateSession(LoginResult result)
        {
            UserSession.Current.AccountId = result.AccountId;
            UserSession.Current.Email = result.Email;
            UserSession.Current.CustomerName = result.DisplayName;
            UserSession.Current.CustomerId = result.CustomerId;
            UserSession.Current.StaffId = result.StaffId;
            UserSession.Current.isStaff = result.Role != "Khách hàng";
        }

        private void Navigate(string role)
        {
            switch (role)
            {
                case "Khách hàng": _navigationService.OpenCustomerWindow(); break;
                case "Nhân viên": _navigationService.OpenStaffWindow(); break;
                case "Admin": _navigationService.OpenAdminWindow(); break;
            }
        }

        [RelayCommand]
        private void ShowRegister() => _auth.NavigateToRegister();

        [RelayCommand]
        private void ShowForgotPassword() => _auth.NavigateToForgotPassword();
    }
}
