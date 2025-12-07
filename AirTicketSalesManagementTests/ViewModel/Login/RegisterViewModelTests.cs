using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Register;
using AirTicketSalesManagement.Services.Timer;
using AirTicketSalesManagement.ViewModel.Login;
using AirTicketSalesManagement.ViewModel;
using Moq;
using System.Windows.Media;

namespace AirTicketSalesManagementTests.ViewModel.Login
{

    [TestFixture]
    public class RegisterViewModelTests
    {
        [TestFixture]
        public class RegisterTests
        {
            private Mock<IRegisterService> _registerServiceMock;
            private Mock<IEmailService> _emailServiceMock;
            private Mock<IOtpService> _otpServiceMock;
            private Mock<IEmailTemplateService> _emailTemplateServiceMock;
            private Mock<ITimerService> _timerServiceMock;
            private Mock<IEmailValidation> _emailValidationMock;
            private Mock<ToastViewModel> _toastViewModelMock;
            private Mock<AuthViewModel> _authViewModelMock;
            private RegisterViewModel _viewModel;

            [SetUp]
            public void SetUp()
            {
                _registerServiceMock = new Mock<IRegisterService>();
                _emailServiceMock = new Mock<IEmailService>();
                _otpServiceMock = new Mock<IOtpService>();
                _emailTemplateServiceMock = new Mock<IEmailTemplateService>();
                _timerServiceMock = new Mock<ITimerService>();
                _emailValidationMock = new Mock<IEmailValidation>();
                _toastViewModelMock = new Mock<ToastViewModel>();
                _authViewModelMock = new Mock<AuthViewModel>();

                var toastViewModel = new ToastViewModel();


                _viewModel = new RegisterViewModel(
                    _authViewModelMock.Object,
                    _registerServiceMock.Object,
                    _emailServiceMock.Object,
                    _otpServiceMock.Object,
                    _emailTemplateServiceMock.Object,
                    _timerServiceMock.Object,
                    _emailValidationMock.Object,
                    toastViewModel
                );
            }

            [Test]
            public async Task Register_ShouldShowError_WhenNameIsEmpty()
            {
                _viewModel.Name = "";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Name));
                CollectionAssert.Contains(errors, "Tên không được để trống.");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenNameIsTooLong()
            {
                _viewModel.Name = new string('a', 31);
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Name));
                CollectionAssert.Contains(errors, "Tên vượt quá giới hạn cho phép");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenEmailIsEmpty()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Email không được để trống.");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenEmailIsTooLong()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = new string('a', 245) + "@gmail.com"; // 255 chars
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Email vượt quá giới hạn cho phép");
            }

            [TestCase("ngocvo2502")]
            [TestCase("ngocvo2502@")]
            [TestCase("ngocvo2502@gmail")]
            [TestCase("ngocvo2502@.com")]
            [TestCase("@gmail.com")]
            public async Task Register_ShouldShowError_WhenEmailIsInvalid(string email)
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = email;
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(email)).Returns(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Email không hợp lệ.");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenEmailAlreadyExists()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "yry12333@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(true);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Email đã được đăng ký");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenPasswordIsEmpty()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Password));
                CollectionAssert.Contains(errors, "Mật khẩu không được để trống.");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenPasswordIsTooLong()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = new string('a', 101);
                _viewModel.ConfirmPassword = new string('a', 101);
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Password));
                CollectionAssert.Contains(errors, "Mật khẩu vượt quá giới hạn cho phép");
            }

            [Test]
            public async Task Register_ShouldShowError_WhenConfirmPasswordDoesNotMatch()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "123";
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.ConfirmPassword));
                CollectionAssert.Contains(errors, "Xác nhận mật khẩu không khớp với mật khẩu.");
            }

            [TestCase("ngocvo2502@gmail.com")]
            [TestCase("NGOCVO2502@ANONYVIET.COM.VN")]
            public async Task Register_ShouldSendOtpAndShowToast_WhenValid(string email)
            {
                var toastViewModel = new FakeToastViewModel();
                _viewModel = new RegisterViewModel(
                    _authViewModelMock.Object,
                    _registerServiceMock.Object,
                    _emailServiceMock.Object,
                    _otpServiceMock.Object,
                    _emailTemplateServiceMock.Object,
                    _timerServiceMock.Object,
                    _emailValidationMock.Object,
                    toastViewModel
                );

                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = email;
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);
                _otpServiceMock.Setup(x => x.GenerateOtp(_viewModel.Email)).Returns("123456");
                _emailTemplateServiceMock.Setup(x => x.BuildRegisterOtp("123456")).Returns("OTP: 123456");
                _emailServiceMock.Setup(x => x.SendEmailAsync(_viewModel.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.AreEqual("Mã xác nhận đã được gửi đến email của bạn.", toastViewModel.LastMessage);
                Assert.IsTrue(_viewModel.IsOtpStep);
            }


            // Additional edge case: ConfirmPassword is empty
            [Test]
            public async Task Register_ShouldShowError_WhenConfirmPasswordIsEmpty()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "";
                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.ConfirmPassword));
                CollectionAssert.Contains(errors, "Xác nhận mật khẩu không khớp với mật khẩu.");
            }

            [Test]
            public async Task Register_ShouldNotSendOtp_WhenValidationFails()
            {
                // gây lỗi: Name rỗng
                _viewModel.Name = "";
                _viewModel.Email = "valid@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";

                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                // VERIFY: Không gửi email
                _emailServiceMock.Verify(
                    x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                    Times.Never);

                // VERIFY: Không tạo OTP
                _otpServiceMock.Verify(x => x.GenerateOtp(It.IsAny<string>()), Times.Never);

                // VERIFY: Không start timer
                _timerServiceMock.Verify(
                    x => x.Start(It.IsAny<TimeSpan>(), It.IsAny<Action<TimeSpan>>(), It.IsAny<Action>()),
                    Times.Never);

                Assert.IsFalse(_viewModel.IsOtpStep);
            }


            [Test]
            public async Task Register_ShouldShowErrorToast_WhenEmailSendingFails()
            {
                var toast = new FakeToastViewModel();

                _viewModel = new RegisterViewModel(
                    _authViewModelMock.Object,
                    _registerServiceMock.Object,
                    _emailServiceMock.Object,
                    _otpServiceMock.Object,
                    _emailTemplateServiceMock.Object,
                    _timerServiceMock.Object,
                    _emailValidationMock.Object,
                    toast
                );

                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "test@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";

                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

                // FORCE SendEmailAsync lỗi
                _emailServiceMock
                    .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception("SMTP failed"));

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                Assert.AreEqual("Không thể gửi mã xác nhận. Vui lòng thử lại sau.", toast.LastMessage);
                Assert.IsTrue(_viewModel.IsOtpStep); // vẫn chuyển sang OTP step
            }

            [Test]
            public async Task Register_ShouldStartTimer_WhenValid()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "ngocvo2502@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";

                _emailValidationMock.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

                _otpServiceMock.Setup(x => x.GenerateOtp(It.IsAny<string>())).Returns("123456");
                _emailTemplateServiceMock.Setup(x => x.BuildRegisterOtp("123456")).Returns("email content");
                _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                 .Returns(Task.CompletedTask);

                await _viewModel.RegisterCommand.ExecuteAsync(null);

                _timerServiceMock.Verify(
                    x => x.Start(
                        TimeSpan.FromMinutes(3),
                        It.IsAny<Action<TimeSpan>>(),
                        It.IsAny<Action>()
                    ),
                    Times.Once
                );
            }

            [Test]
            public async Task Register_ShouldPassValidation_WhenAllInputsValid()
            {
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Email = "test@gmail.com";
                _viewModel.Password = "12345678";
                _viewModel.ConfirmPassword = "12345678";

                _emailValidationMock.Setup(x => x.IsValid(_viewModel.Email)).Returns(true);
                _registerServiceMock.Setup(x => x.IsEmailExistsAsync(_viewModel.Email)).ReturnsAsync(false);

                await _viewModel.ValidateAsync();

                Assert.IsFalse(_viewModel.HasErrors);
            }




        }

        [TestFixture]
        public class ConfirmOTPTests
        {
            private Mock<IRegisterService> _registerServiceMock;
            private Mock<IEmailService> _emailServiceMock;
            private Mock<IOtpService> _otpServiceMock;
            private Mock<IEmailTemplateService> _emailTemplateServiceMock;
            private Mock<ITimerService> _timerServiceMock;
            private Mock<IEmailValidation> _emailValidationMock;
            private Mock<AuthViewModel> _authViewModelMock;
            private FakeToastViewModel _toastViewModel;
            private RegisterViewModel _viewModel;

            [SetUp]
            public void SetUp()
            {
                _registerServiceMock = new Mock<IRegisterService>();
                _emailServiceMock = new Mock<IEmailService>();
                _otpServiceMock = new Mock<IOtpService>();
                _emailTemplateServiceMock = new Mock<IEmailTemplateService>();
                _timerServiceMock = new Mock<ITimerService>();
                _emailValidationMock = new Mock<IEmailValidation>();
                _authViewModelMock = new Mock<AuthViewModel>();
                _toastViewModel = new FakeToastViewModel();

                _viewModel = new RegisterViewModel(
                    _authViewModelMock.Object,
                    _registerServiceMock.Object,
                    _emailServiceMock.Object,
                    _otpServiceMock.Object,
                    _emailTemplateServiceMock.Object,
                    _timerServiceMock.Object,
                    _emailValidationMock.Object,
                    _toastViewModel
                )
                {
                    Name = "Ngoc Vo",
                    Email = "ngocvo2502@gmail.com",
                    Password = "12345678"
                };
            }

            [Test]
            public async Task ConfirmOtp_ShouldShowError_WhenOtpCodeIsEmpty()
            {
                _viewModel.OtpCode = "";

                await _viewModel.ConfirmOtpCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.OtpCode));
                CollectionAssert.Contains(errors, "Mã OTP không được để trống.");
            }

            [Test]
            public async Task ConfirmOtp_ShouldShowError_WhenOtpCodeIsInvalid()
            {
                _viewModel.OtpCode = "654321";
                _otpServiceMock.Setup(x => x.VerifyOtp(_viewModel.Email, "654321")).Returns(false);

                await _viewModel.ConfirmOtpCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.OtpCode));
                CollectionAssert.Contains(errors, "Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            [Test]
            public async Task ConfirmOtp_ShouldShowToast_WhenCreateCustomerFails()
            {
                _viewModel.OtpCode = "123456";
                _otpServiceMock.Setup(x => x.VerifyOtp(_viewModel.Email, "123456")).Returns(true);
                _registerServiceMock.Setup(x => x.CreateCustomerAsync(_viewModel.Name, _viewModel.Email, _viewModel.Password)).ReturnsAsync(false);

                await _viewModel.ConfirmOtpCommand.ExecuteAsync(null);

                Assert.AreEqual("Không thể kết nối đến cơ sở dữ liệu", _toastViewModel.LastMessage);
                Assert.AreEqual(Brushes.OrangeRed, _toastViewModel.LastBrush);
            }

            [Test]
            public async Task ConfirmOtp_ShouldShowToastAndNavigate_WhenSuccess()
            {
                // SETUP REQUIRED PROPERTIES
                _viewModel.Email = "test@gmail.com";
                _viewModel.Name = "Ngoc Vo";
                _viewModel.Password = "123456";
                _viewModel.OtpCode = "123456";

                // MOCK OTP SUCCESS
                _otpServiceMock.Setup(x => x.VerifyOtp(_viewModel.Email, "123456"))
                               .Returns(true);

                // MOCK CREATE CUSTOMER SUCCESS
                _registerServiceMock.Setup(x =>
                    x.CreateCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
                ).ReturnsAsync(true);

                bool navigated = false;
                _authViewModelMock.Setup(x => x.NavigateToLogin())
                                  .Callback(() => navigated = true);

                await _viewModel.ConfirmOtpCommand.ExecuteAsync(null);

                Assert.AreEqual("Đăng ký thành công. Vui lòng đăng nhập.", _toastViewModel.LastMessage);
                Assert.AreEqual(Brushes.Green, _toastViewModel.LastBrush);
                Assert.IsTrue(navigated);
            }

        }



        public class FakeToastViewModel : ToastViewModel
        {
            public string LastMessage { get; private set; }
            public Brush LastBrush { get; private set; }
            public override async Task ShowToastAsync(string message, Brush bg = null, int durationMs = 2000)
            {
                LastMessage = message;
                LastBrush = bg;
                await Task.CompletedTask;
            }
        }
    }
}

