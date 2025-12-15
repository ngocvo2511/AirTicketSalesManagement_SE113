using AirTicketSalesManagement.Services.ForgotPassword;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Login;
using Moq;
using NUnit.Framework;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class ForgotPasswordAsync_test
    {
        private Mock<AuthViewModel> _mockAuth;
        private Mock<IForgotPasswordService> _mockForgotPasswordService;
        private Mock<ToastViewModel> _mockToast;
        private ForgotPasswordViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _mockAuth = new Mock<AuthViewModel>();
            _mockForgotPasswordService = new Mock<IForgotPasswordService>();
            _mockToast = new Mock<ToastViewModel>();
            _viewModel = new ForgotPasswordViewModel(_mockAuth.Object, _mockForgotPasswordService.Object, _mockToast.Object);
        }

        public static IEnumerable<TestCaseData> ForgotPasswordTestCases
        {
            get
            {
                yield return new TestCaseData("valid@gmail.com", true, true, true, null);
                yield return new TestCaseData("valid@gmail.com", true, true, false, "Lỗi kết nối cơ sở dữ liệu.");
                yield return new TestCaseData("invalid-email", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("notexist@gmail.com", true, false, true, "Tài khoản không tồn tại.");
                yield return new TestCaseData("ngocvo2502@", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("ngocvo2502@.com", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("valid@gmail.com", true, true, true, null); 
                yield return new TestCaseData("", false, false, true, "Email không được để trống.");
            }
        }

        [Test]
        [TestCaseSource(nameof(ForgotPasswordTestCases))]
        public async Task ForgotPasswordAsync_Test(string email, bool isValidFormat, bool isExist, bool dbConnection, string expectedError)
        {
            _viewModel.Email = email;
            _mockForgotPasswordService.Setup(s => s.IsValid(email)).Returns(isValidFormat);
            if (dbConnection)
                _mockForgotPasswordService.Setup(s => s.EmailExistsAsync(email)).ReturnsAsync(isExist);
            else
                _mockForgotPasswordService.Setup(s => s.EmailExistsAsync(email)).ThrowsAsync(new Exception("DB Error"));

            await _viewModel.ForgotPasswordCommand.ExecuteAsync(null);

            if (expectedError != null)
            {
                if (expectedError.Contains("Lỗi kết nối"))
                    _mockToast.Verify(t => t.ShowToastAsync(It.Is<string>(s => s.Contains(expectedError)), null, 2000), Times.Once);
                else
                {
                    var errors = _viewModel.GetErrors(nameof(_viewModel.Email)).Cast<string>();
                    Assert.That(errors, Does.Contain(expectedError));
                }
            }
            else
            {
                Assert.IsFalse(_viewModel.HasErrors);
            }
        }
    }
}
