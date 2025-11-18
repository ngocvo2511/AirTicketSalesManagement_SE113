using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Register;
using AirTicketSalesManagement.Services.Timer;
using BCrypt.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace AirTicketSalesManagement.ViewModel.Login
{
    //public partial class RegisterViewModel : BaseViewModel, INotifyDataErrorInfo
    //{
    //    private readonly IEmailService _emailService;
    //    private readonly IOtpService _otpService;
    //    private readonly IEmailTemplateService _emailTemplateService;
    //    private readonly AuthViewModel _auth;
    //    private readonly Dictionary<string, List<string>> _errors = new();
    //    public ToastViewModel Toast { get; }
    //    private bool isFailed;
    //    [ObservableProperty]
    //    private string email;
    //    [ObservableProperty]
    //    private string password;
    //    [ObservableProperty]
    //    private string confirmPassword;
    //    [ObservableProperty]
    //    private string name;
    //    [ObservableProperty]
    //    private bool isOtpStep = false;
    //    [ObservableProperty]
    //    private string otpCode;
    //    [ObservableProperty]
    //    private string? timeLeftText;

    //    private DispatcherTimer? _timer;
    //    private TimeSpan _timeLeft;

    //    #region Error
    //    public async Task Validate()
    //    {
    //        ClearErrors(nameof(Email));
    //        ClearErrors(nameof(Password));
    //        ClearErrors(nameof(ConfirmPassword));
    //        ClearErrors(nameof(Name));
    //        if (string.IsNullOrWhiteSpace(Name))
    //            AddError(nameof(Name), "Tên không được để trống.");
    //        else if (Name.Length > 30)
    //            AddError(nameof(Name), "Tên vượt quá giới hạn cho phép");
    //        if (string.IsNullOrWhiteSpace(Email))
    //            AddError(nameof(Email), "Email không được để trống.");
    //        else if (Email.Length > 254)
    //            AddError(nameof(Name), "Email vượt quá giới hạn cho phép");
    //        else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
    //            AddError(nameof(Email), "Email không hợp lệ.");

    //        if (string.IsNullOrWhiteSpace(Password))
    //            AddError(nameof(Password), "Mật khẩu không được để trống.");
    //        else if (Password.Length > 100)
    //            AddError(nameof(Password), "Mật khẩu vượt quá giới hạn cho phép");
    //        if (ConfirmPassword != Password)
    //        {
    //            AddError(nameof(ConfirmPassword), "Xác nhận mật khẩu không khớp với mật khẩu.");
    //        }
    //        if (HasErrors) return;
    //        try
    //        {
    //            using (var context = new AirTicketDbContext())
    //            {
    //                var user = context.Taikhoans.FirstOrDefault(x => x.Email == Email);
    //                if (user != null)
    //                {
    //                    AddError(nameof(Email), "Email đã được đăng kí");
    //                    return;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            await Toast.ShowToastAsync("Không thể kết nối đến cơ sở dữ liệu", Brushes.OrangeRed);
    //        }

    //    }
    //    public bool HasErrors => _errors.Any();
    //    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    //    public IEnumerable? GetErrors(string propertyName)
    //    {
    //        if (!string.IsNullOrWhiteSpace(propertyName) && _errors.ContainsKey(propertyName))
    //            return _errors[propertyName];
    //        return null;
    //    }

    //    private void AddError(string propertyName, string error)
    //    {
    //        if (!_errors.ContainsKey(propertyName))
    //            _errors[propertyName] = new List<string>();

    //        if (!_errors[propertyName].Contains(error))
    //        {
    //            _errors[propertyName].Add(error);
    //            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    //        }
    //    }

    //    private void ClearErrors(string propertyName)
    //    {
    //        if (_errors.ContainsKey(propertyName))
    //        {
    //            _errors.Remove(propertyName);
    //            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    //        }
    //    }

    //    #endregion

    //    #region add account

    //    public async Task AddCustomer()
    //    {
    //        if (isFailed)
    //        {
    //            await Toast.ShowToastAsync("Không thể kết nối đến cơ sở dữ liệu", Brushes.OrangeRed);
    //            return;
    //        }
    //        try
    //        {
    //            using (var context = new AirTicketDbContext())
    //            {
    //                string hashPass = BCrypt.Net.BCrypt.HashPassword(Password);
    //                var customer = new Khachhang
    //                {
    //                    HoTenKh = Name
    //                };
    //                var customerAccount = new Taikhoan
    //                {
    //                    Email = Email,
    //                    VaiTro = "Khách hàng",
    //                    MatKhau = hashPass,
    //                    MaKhNavigation = customer
    //                };
    //                context.Khachhangs.Add(customer);
    //                context.Taikhoans.Add(customerAccount);
    //                context.SaveChanges();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine(ex);
    //            isFailed = true;
    //            await Toast.ShowToastAsync("Không thể kết nối đến cơ sở dữ liệu", Brushes.OrangeRed);
    //            return;
    //        }

    //    }
    //    #endregion
    //    public RegisterViewModel()
    //    {
    //        // Default constructor
    //    }

    //    public RegisterViewModel(AuthViewModel auth, IEmailService emailService, IOtpService otpService, IEmailTemplateService emailTemplateService, ToastViewModel toast)
    //    {
    //        _auth = auth;
    //        _emailService = emailService;
    //        _otpService = otpService;
    //        _emailTemplateService = emailTemplateService;
    //        Toast = toast;
    //    }

    //    [RelayCommand]
    //    private async Task Register()
    //    {
    //        await Validate();
    //        if (HasErrors) return;
    //        IsOtpStep = true;
    //        string otp = _otpService.GenerateOtp(Email);
    //        var emailContent = _emailTemplateService.BuildRegisterOtp(otp);
    //        try
    //        {
    //            StartCountdown();
    //            await _emailService.SendEmailAsync(Email, "Yêu cầu đăng kí tài khoản", emailContent);
    //            await Toast.ShowToastAsync("Mã xác nhận đã được gửi đến email của bạn.", Brushes.Green);
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine(ex);
    //            await Toast.ShowToastAsync("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", Brushes.OrangeRed);
    //            return;
    //        }
    //    }

    //    [RelayCommand]
    //    private void ShowLogin() => _auth.NavigateToLogin();

    //    [RelayCommand]
    //    private async Task ConfirmOtp()
    //    {
    //        ClearErrors(nameof(OtpCode));
    //        if (string.IsNullOrWhiteSpace(OtpCode))
    //        {
    //            AddError(nameof(OtpCode), "Mã OTP không được để trống.");
    //            return;
    //        }
    //        if (_otpService.VerifyOtp(Email, OtpCode))
    //        {
    //            await AddCustomer();
    //            if (isFailed) return;
    //            await Toast.ShowToastAsync("Đăng kí thành công. Vui lòng đăng nhập.", Brushes.Green);
    //            _auth.NavigateToLogin();
    //        }
    //        else
    //        {
    //            AddError(nameof(OtpCode), "Mã OTP không hợp lệ hoặc đã hết hạn.");
    //        }
    //    }
    //    [RelayCommand]
    //    private async Task ResendOtp()
    //    {
    //        string otp = _otpService.GenerateOtp(Email);
    //        var emailContent = _emailTemplateService.BuildRegisterOtp(otp);
    //        try
    //        {
    //            await _emailService.SendEmailAsync(Email, "Yêu cầu đặt lại mật khẩu", emailContent);
    //            await Toast.ShowToastAsync("Mã xác nhận đã được gửi đến email của bạn.", Brushes.Green);
    //            ResetCountdown();
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine(ex);
    //            await Toast.ShowToastAsync("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", Brushes.OrangeRed);
    //        }
    //    }
    //    private void StartCountdown()
    //    {
    //        _timeLeft = TimeSpan.FromMinutes(3);
    //        UpdateTimeLeftText();

    //        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    //        _timer.Tick += (_, _) =>
    //        {
    //            _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
    //            UpdateTimeLeftText();

    //            if (_timeLeft <= TimeSpan.Zero)
    //            {
    //                _timer?.Stop();
    //                TimeLeftText = "Mã xác nhận đã hết hạn.";
    //            }
    //        };
    //        _timer.Start();
    //    }

    //    private void ResetCountdown()
    //    {
    //        _timer?.Stop();
    //        StartCountdown();
    //    }

    //    private void UpdateTimeLeftText()
    //    {
    //        TimeLeftText = $"Mã hết hạn sau: {_timeLeft.Minutes:D2}:{_timeLeft.Seconds:D2}";
    //    }
    //    private void ResetFields()
    //    {
    //        Email = string.Empty;
    //        Password = string.Empty;
    //        ConfirmPassword = string.Empty;
    //        Name = string.Empty;
    //        OtpCode = string.Empty;
    //        IsOtpStep = false;
    //        TimeLeftText = string.Empty;
    //        _timer?.Stop();
    //    }
    //}
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
            else if (_emailValidation.IsValid(Email))
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
