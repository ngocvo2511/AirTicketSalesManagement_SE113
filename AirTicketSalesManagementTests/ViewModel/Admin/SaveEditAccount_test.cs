using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq.Expressions;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class SaveEditAccount_test
    {
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<AirTicketDbContext> _mockContext;
        private AccountManagementViewModel _viewModel;
        private Taikhoan _accountStub;

        [SetUp]
        public void Setup()
        {
            AirTicketSalesManagement.Services.UserSession.Current.AccountId = 1;

            _accountStub = new Taikhoan
            {
                MaTk = 2,
                Email = "staff@example.com",
                VaiTro = "Nhân viên",
                MaNv = 1
            };

            var listAccount = new List<Taikhoan>
    {
        _accountStub,
        new Taikhoan { MaTk = 3, Email = "exist@example.com" }
    };

            _mockContext = new Mock<AirTicketDbContext>();

            // ✅ DbSet async-safe
            _mockContext.Setup(c => c.Taikhoans)
                .ReturnsDbSet(listAccount);

            _mockContext.Setup(c => c.SaveChanges())
                .Returns(1);

            _mockDbContextService = new Mock<IAirTicketDbContextService>();
            _mockDbContextService
                .Setup(s => s.CreateDbContext())
                .Returns(_mockContext.Object);

            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService
                .Setup(n => n.ShowNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<NotificationType>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(true);

            _viewModel = new AccountManagementViewModel(
                _mockDbContextService.Object,
                _mockNotificationService.Object);

            _viewModel.SelectedAccount = new AccountModel
            {
                Id = 2,
                Email = "staff@example.com",
                VaiTro = "Nhân viên",
                HoTen = "Staff Name"
            };
        }


        public static IEnumerable<TestCaseData> SaveEditAccountTestCases
        {
            get
            {
                yield return new TestCaseData("valid@gmail.com", "Admin", "Test Name", null, "Tài khoản đã được cập nhật thành công!", NotificationType.Information);
                yield return new TestCaseData("invalid-email", "Nhân viên", "Test Name", null, "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("", "Nhân viên", "Test Name", null, "Vui lòng điền đầy đủ thông tin tài khoản.", NotificationType.Warning);
                yield return new TestCaseData("exist@example.com", "Nhân viên", "Test Name", null, "Email này đã được sử dụng.", NotificationType.Warning);
                yield return new TestCaseData("valid@gmail.com", "", "Test Name", null, "Vui lòng điền đầy đủ thông tin tài khoản.", NotificationType.Warning);
                yield return new TestCaseData("valid@gmail.com", "Nhân viên", "", null, "Vui lòng điền đầy đủ thông tin tài khoản.", NotificationType.Warning);
                yield return new TestCaseData("valid@gmail.com", "Nhân viên", "Test Name", "newpass", "Tài khoản đã được cập nhật thành công!", NotificationType.Information);
                yield return new TestCaseData("valid@gmail.com", "Nhân viên", "Test Name", null, "Không tìm thấy tài khoản để chỉnh sửa.", NotificationType.Error);
                yield return new TestCaseData("valid@gmail.com", "Khách hàng", "Test Name", null, "Không thể chỉnh sửa vai trò tài khoản của bạn.", NotificationType.Warning);
            }
        }

        [Test]
        [TestCaseSource(nameof(SaveEditAccountTestCases))]
        public async Task SaveEditAccount_Test(string email, string role, string fullname, string password, string expectedMessage, NotificationType expectedType)
        {
            _viewModel.EditEmail = email;
            _viewModel.EditRole = role;
            _viewModel.EditFullName = fullname;
            _viewModel.EditPassword = password;

            if (expectedMessage.Contains("Không tìm thấy"))
                _viewModel.SelectedAccount.Id = 99; 
            
            if (expectedMessage.Contains("tài khoản của bạn"))
            {
                _viewModel.SelectedAccount.Id = 1; 
                _accountStub.MaTk = 1; 
                _viewModel.SelectedAccount.VaiTro = "Admin"; 
            }

            _viewModel.SaveEditAccountCommand.Execute(null);

            _mockNotificationService.Verify(n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains(expectedMessage)), expectedType, It.IsAny<bool>()), Times.Once);
        }
    }
}