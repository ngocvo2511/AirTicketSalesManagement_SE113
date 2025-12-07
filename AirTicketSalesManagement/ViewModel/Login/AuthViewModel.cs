using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.ForgotPassword;
using AirTicketSalesManagement.Services.Login;
using AirTicketSalesManagement.Services.Navigation;
using AirTicketSalesManagement.Services.Register;
using AirTicketSalesManagement.Services.Timer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AirTicketSalesManagement.ViewModel.Login
{
    public partial class AuthViewModel : BaseViewModel
    {
        [ObservableProperty]
        private BaseViewModel currentViewModel;

        public AuthViewModel()
        {
            CurrentViewModel = new LoginViewModel(this, new LoginService(new Data.AirTicketDbContext()), new NavigationWindowService(), new EmailValidation(), new ToastViewModel());
        }

        
        [RelayCommand]
        private void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        public void NavigateToRegister()
        {
            CurrentViewModel = new RegisterViewModel(this, new RegisterService(new Data.AirTicketDbContext()), new EmailService(), new OtpService(), new EmailTemplateService(), new DispatcherTimerService(), new EmailValidation(), new ToastViewModel());
        }

        public virtual void NavigateToLogin()
        {
            CurrentViewModel = new LoginViewModel(this,new LoginService(new Data.AirTicketDbContext()), new NavigationWindowService(), new EmailValidation(), new ToastViewModel());
        }

        public void NavigateToForgotPassword()
        {
            CurrentViewModel = new ForgotPasswordViewModel(this, new ForgotPasswordService(new Data.AirTicketDbContext()), new ToastViewModel());
        }
    }
}
