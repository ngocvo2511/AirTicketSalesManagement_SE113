using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.ResetPassword;
using AirTicketSalesManagement.Services.Timer;
using AirTicketSalesManagement.ViewModel.Login;
using AirTicketSalesManagement.ViewModel;
using Moq;
using System.Windows.Media;

namespace AirTicketSalesManagementTests.ViewModel.Login
{
    [TestFixture]
    public class ResetPasswordViewModelTests
    {
        private Mock<IResetPasswordService> _resetPasswordServiceMock;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IOtpService> _otpServiceMock;
        private Mock<IEmailTemplateService> _emailTemplateServiceMock;
        private Mock<ITimerService> _timerServiceMock;
        private AuthViewModel _authViewModel;
        private FakeToastViewModel _toastViewModel;
        private ResetPasswordViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _resetPasswordServiceMock = new Mock<IResetPasswordService>();
            _emailServiceMock = new Mock<IEmailService>();
            _otpServiceMock = new Mock<IOtpService>();
            _emailTemplateServiceMock = new Mock<IEmailTemplateService>();
            _timerServiceMock = new Mock<ITimerService>();
            _authViewModel = new AuthViewModel();
            _toastViewModel = new FakeToastViewModel();

            _viewModel = new ResetPasswordViewModel(
                _authViewModel,
                "test@gmail.com",
                _resetPasswordServiceMock.Object,
                _emailServiceMock.Object,
                _otpServiceMock.Object,
                _emailTemplateServiceMock.Object,
                _timerServiceMock.Object,
                _toastViewModel
            );
        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowError_WhenIsCodeValidIsFalse()
        {
            _viewModel.IsCodeValid = false;
            _viewModel.Password = "123456";
            _viewModel.ConfirmPassword = "123456";

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.Code));
            CollectionAssert.Contains(errors, "Vui lòng xác nhận mã trước khi đặt lại mật khẩu.");
        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowError_WhenPasswordIsNull()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = null;
            _viewModel.ConfirmPassword = "asdf";

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.Password));
            CollectionAssert.Contains(errors, "Mật khẩu không được để trống.");
            _resetPasswordServiceMock.Verify(
    x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowError_WhenPasswordIsEmpty()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = "";
            _viewModel.ConfirmPassword = "123456";

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.Password));
            CollectionAssert.Contains(errors, "Mật khẩu không được để trống.");

            _resetPasswordServiceMock.Verify(
    x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowError_WhenPasswordIsTooLong()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = new string('a', 101);
            _viewModel.ConfirmPassword = new string('a', 101);

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.Password));
            CollectionAssert.Contains(errors, "Mật khẩu vượt quá giới hạn cho phép");

            _resetPasswordServiceMock.Verify(
    x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowError_WhenConfirmPasswordDoesNotMatch()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = "123456";
            _viewModel.ConfirmPassword = "asdf";

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.ConfirmPassword));
            CollectionAssert.Contains(errors, "Xác nhận mật khẩu không khớp với mật khẩu.");

            _resetPasswordServiceMock.Verify(
    x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public async Task ResetPasswordAsync_ShouldUpdatePassword_WhenValid()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = "123456";
            _viewModel.ConfirmPassword = "123456";
            _resetPasswordServiceMock.Setup(x => x.UpdatePasswordAsync(_viewModel.Email, "123456")).Returns(Task.CompletedTask);

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            // Đã gọi UpdatePasswordAsync
            _resetPasswordServiceMock.Verify(x => x.UpdatePasswordAsync(_viewModel.Email, "123456"), Times.Once);
            // Đã chuyển về LoginViewModel
            Assert.IsInstanceOf<LoginViewModel>(_authViewModel.CurrentViewModel);

            Assert.AreEqual("Mật khẩu đã được thay đổi thành công", _toastViewModel.LastMessage);
            Assert.AreEqual(Brushes.Green, _toastViewModel.LastBrush);
        }

        [Test]
        public async Task ResetPasswordAsync_ShouldShowToast_WhenUpdatePasswordFails()
        {
            _viewModel.IsCodeValid = true;
            _viewModel.Password = "123456";
            _viewModel.ConfirmPassword = "123456";
            _resetPasswordServiceMock.Setup(x => x.UpdatePasswordAsync(_viewModel.Email, "123456")).ThrowsAsync(new System.Exception("DB error"));

            await _viewModel.ResetPasswordCommand.ExecuteAsync(null);

            Assert.AreEqual("Không thể kết nối đến cơ sở dữ liệu", _toastViewModel.LastMessage);
            Assert.AreEqual(Brushes.OrangeRed, _toastViewModel.LastBrush);
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
