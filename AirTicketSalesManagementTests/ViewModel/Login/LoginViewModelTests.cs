using AirTicketSalesManagement.Services.EmailValidation;
using AirTicketSalesManagement.Services.Login;
using AirTicketSalesManagement.Services.Navigation;
using AirTicketSalesManagement.ViewModel.Login;
using AirTicketSalesManagement.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests.ViewModel.Login
{
    [TestFixture]
    public class LoginViewModelTests
    {
        [TestFixture]
        public class LoginTests()
        {
            private Mock<AuthViewModel> _authMock;
            private Mock<ILoginService> _loginServiceMock;
            private Mock<INavigationService> _navigationServiceMock;
            private Mock<IEmailValidation> _emailValidationMock;
            private ToastViewModel _toastViewModel;
            private LoginViewModel _viewModel;

            [SetUp]
            public void SetUp()
            {
                _authMock = new Mock<AuthViewModel>();
                _loginServiceMock = new Mock<ILoginService>();
                _navigationServiceMock = new Mock<INavigationService>();
                _emailValidationMock = new Mock<IEmailValidation>();
                _toastViewModel = new ToastViewModel();

                _viewModel = new LoginViewModel(
                    _authMock.Object,
                    _loginServiceMock.Object,
                    _navigationServiceMock.Object,
                    _emailValidationMock.Object,
                    _toastViewModel
                );
            }

            [TestCase("customer@gmail.com", "customer", "Khách hàng")]
            [TestCase("staff@gmail.com", "staff", "Nhân viên")]
            [TestCase("admin@gmail.com", "admin", "Admin")]
            public async Task Login_ShouldNavigate_WhenLoginSuccess(string email, string password, string role)
            {
                _viewModel.Email = email;
                _viewModel.Password = password;
                _emailValidationMock.Setup(x => x.IsValid(email)).Returns(true);

                _loginServiceMock.Setup(x => x.LoginAsync(email, password)).ReturnsAsync(new LoginResult
                {
                    Success = true,
                    Role = role,
                    AccountId = 1,
                    CustomerId = role == "Khách hàng" ? 10 : null,
                    StaffId = role != "Khách hàng" ? 20 : null,
                    DisplayName = "Test User",
                    Email = email
                });

                bool customerNavCalled = false, staffNavCalled = false, adminNavCalled = false;
                _navigationServiceMock.Setup(x => x.OpenCustomerWindow()).Callback(() => customerNavCalled = true);
                _navigationServiceMock.Setup(x => x.OpenStaffWindow()).Callback(() => staffNavCalled = true);
                _navigationServiceMock.Setup(x => x.OpenAdminWindow()).Callback(() => adminNavCalled = true);

                await _viewModel.LoginCommand.ExecuteAsync(null);

                Assert.IsFalse(_viewModel.HasErrors);

                switch (role)
                {
                    case "Khách hàng":
                        Assert.IsTrue(customerNavCalled);
                        break;
                    case "Nhân viên":
                        Assert.IsTrue(staffNavCalled);
                        break;
                    case "Admin":
                        Assert.IsTrue(adminNavCalled);
                        break;
                }
            }

            [TestCase("test@gmail.com", "customer")]
            [TestCase("customer@gmail.com", "staff")]
            [TestCase("staff@gmail.com", "admin")]
            [TestCase("admin@gmail.com", "")]
            public async Task Login_ShouldShowError_WhenLoginFailed(string email, string password)
            {
                _viewModel.Email = email;
                _viewModel.Password = password;
                _emailValidationMock.Setup(x => x.IsValid(email)).Returns(true);

                _loginServiceMock.Setup(x => x.LoginAsync(email, password)).ReturnsAsync(new LoginResult
                {
                    Success = false,
                    Error = "Tài khoản hoặc mật khẩu không hợp lệ"
                });

                await _viewModel.LoginCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Tài khoản hoặc mật khẩu không hợp lệ");
            }

            [TestCase("", "customer")]
            [TestCase("customer@gmail.com", "")]
            public async Task Login_ShouldShowError_WhenEmailOrPasswordIsEmpty(string email, string password)
            {
                _viewModel.Email = email;
                _viewModel.Password = password;
                _emailValidationMock.Setup(x => x.IsValid(email)).Returns(!string.IsNullOrWhiteSpace(email));

                await _viewModel.LoginCommand.ExecuteAsync(null);

                Assert.IsTrue(_viewModel.HasErrors);
                var errors = _viewModel.GetErrors(nameof(_viewModel.Email));
                CollectionAssert.Contains(errors, "Tài khoản hoặc mật khẩu không hợp lệ");
            }


        }
    }
}
