using AirTicketSalesManagement.Interface; // Thêm namespace chứa các interface
using AirTicketSalesManagement.Services.EmailServices; // Thêm namespace chứa EmailTemplateService/Interface
using AirTicketSalesManagement.Services.ResetPassword;
using AirTicketSalesManagement.Services.Timer;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Login;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class CheckCode_test
    {
        private Mock<IResetPasswordService> _mockResetPasswordService;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IOtpService> _mockOtpService;
        private Mock<IEmailTemplateService> _mockEmailTemplateService;
        private Mock<ITimerService> _mockTimerService;
        private Mock<ToastViewModel> _mockToast;
        private ResetPasswordViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _mockResetPasswordService = new Mock<IResetPasswordService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockOtpService = new Mock<IOtpService>();
            _mockEmailTemplateService = new Mock<IEmailTemplateService>();
            _mockTimerService = new Mock<ITimerService>();
            _mockToast = new Mock<ToastViewModel>();

            _viewModel = new ResetPasswordViewModel(
                new AuthViewModel(), // AuthViewModel có thể cần mock hoặc instance rỗng
                "test@gmail.com", 
                _mockResetPasswordService.Object, 
                _mockEmailService.Object, 
                _mockOtpService.Object, 
                _mockEmailTemplateService.Object, 
                _mockTimerService.Object, 
                _mockToast.Object);
        }

        public static IEnumerable<TestCaseData> CheckCodeTestCases
        {
            get
            {
                yield return new TestCaseData("123456", false, true, null);
                yield return new TestCaseData("123456", false, false, "Mã xác nhận không hợp lệ hoặc đã hết hạn.");
                yield return new TestCaseData("123456", true, false, "Mã xác nhận đã hết hạn. Vui lòng gửi lại mã mới.");
                yield return new TestCaseData("", false, false, "Mã xác nhận không được để trống.");
                yield return new TestCaseData(null, false, false, "Mã xác nhận không được để trống.");
            }
        }

        [Test]
        [TestCaseSource(nameof(CheckCodeTestCases))]
        public async Task CheckCode_Test(string code, bool isExpired, bool isValidOtp, string expectedError)
        {
            _viewModel.Code = code;
            _viewModel.IsCodeExpired = isExpired;
            
            // Fix CS1503: Moq Returns expects a value of type bool, isValidOtp is bool. 
            // If verifyOtp returns bool, this is correct. Ensure types match.
            _mockOtpService.Setup(s => s.VerifyOtp(It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(isValidOtp);

            await _viewModel.CheckCodeCommand.ExecuteAsync(null);

            if (expectedError != null)
            {
                // Fix: GetErrors returns IEnumerable, need to cast or convert to check content
                var errors = _viewModel.GetErrors(nameof(_viewModel.Code)).Cast<string>();
                Assert.That(errors, Does.Contain(expectedError));
            }
            else
            {
                Assert.IsTrue(_viewModel.IsCodeValid);
                Assert.IsFalse(_viewModel.HasErrors);
            }
        }
    }
}