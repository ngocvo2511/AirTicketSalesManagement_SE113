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
                yield return new TestCaseData("customer@gmail.com", true, true, true, null);
                yield return new TestCaseData("", true, true, true, "Email không được để trống.");

                // INVALID FORMAT
                yield return new TestCaseData("ngocvo2502", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("ngocvo2502@", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("ngocvo2502@gmail", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("ngocvo2502@.com", false, false, true, "Email không hợp lệ.");
                yield return new TestCaseData("@gmail.com", false, false, true, "Email không hợp lệ.");

                // NOT EXISTS
                yield return new TestCaseData("test@gmail.com", true, false, true, "Tài khoản không tồn tại.");

                // DB ERROR
                yield return new TestCaseData("user@gmail.com", true, true, false, "Lỗi kết nối cơ sở dữ liệu.");
            }
        }



        [Test]
        [TestCaseSource(nameof(ForgotPasswordTestCases))]
        public async Task ForgotPasswordAsync_Test(
            string email,
            bool isValidFormat,
            bool isExist,
            bool dbConnection,
            string expectedError)
        {
            _viewModel.Email = email;

            _mockForgotPasswordService
                .Setup(s => s.IsValid(email))
                .Returns(isValidFormat);

            if (dbConnection)
                _mockForgotPasswordService
                    .Setup(s => s.EmailExistsAsync(email))
                    .ReturnsAsync(isExist);
            else
                _mockForgotPasswordService
                    .Setup(s => s.EmailExistsAsync(email))
                    .ThrowsAsync(new Exception("DB Error"));

            await _viewModel.ForgotPasswordAsync();

            // CASE 1: DB ERROR → TOAST
            if (expectedError == "Lỗi kết nối cơ sở dữ liệu.")
            {
                _mockToast.Verify(
                    t => t.ShowToastAsync(
                        It.Is<string>(s => s.Contains(expectedError)),
                        null,
                        2000),
                    Times.Once);
                return;
            }

            // CASE 2: VALIDATION ERROR
            if (expectedError != null)
            {
                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email)).Cast<string>();
                Assert.That(errors, Does.Contain(expectedError));
                return;
            }

            // CASE 3: SUCCESS
            Assert.IsFalse(_viewModel.HasErrors);
        }

    }
}
