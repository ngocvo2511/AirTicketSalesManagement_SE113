using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Customer;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System.Globalization;
using System.Linq.Expressions;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class SaveProfile_test
    {
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<AirTicketDbContext> _mockContext;
        private CustomerProfileViewModel _viewModel;
        private Khachhang _khachHangStub;
        private Taikhoan _taiKhoanStub;

        [SetUp]
        public void Setup()
        {
            AirTicketSalesManagement.Services.UserSession.Current.CustomerId = 1;
            _khachHangStub = new Khachhang { MaKh = 1, HoTenKh = "Old Name", SoDt = "0900000000", Cccd = "000000000000", GioiTinh = "Nam", NgaySinh = new DateOnly(1990, 1, 1) };
            _taiKhoanStub = new Taikhoan { MaKh = 1, Email = "yry12333@gmail.com" };

            _mockContext = new Mock<AirTicketDbContext>();
            _mockContext.Setup(c => c.Khachhangs)
                .ReturnsDbSet(new List<Khachhang> { _khachHangStub });

            _mockContext.Setup(c => c.Taikhoans)
                .ReturnsDbSet(new List<Taikhoan>
                {
                    _taiKhoanStub,
                    new Taikhoan
                    {
                        MaKh = 2,
                        Email = "yry12333@gmail.com"
                    }
                });

            _mockContext.Setup(c => c.SaveChangesAsync(default))
                .ReturnsAsync(1);

            _mockDbContextService = new Mock<IAirTicketDbContextService>();
            _mockDbContextService.Setup(s => s.CreateDbContext()).Returns(_mockContext.Object);
            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>())).ReturnsAsync(true);

            _viewModel = new CustomerProfileViewModel(_mockDbContextService.Object, _mockNotificationService.Object);
            _viewModel.EditHoTen = _khachHangStub.HoTenKh;
            _viewModel.EditEmail = _taiKhoanStub.Email;
            _viewModel.EditSoDienThoai = _khachHangStub.SoDt;
            _viewModel.EditCanCuoc = _khachHangStub.Cccd;
            _viewModel.EditGioiTinh = _khachHangStub.GioiTinh;
            _viewModel.EditNgaySinh = _khachHangStub.NgaySinh.Value.ToDateTime(TimeOnly.MinValue);
        }

        public static IEnumerable<TestCaseData> SaveProfileTestCases
        {
            get
            {
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "052205003846", "11/02/2005", "Nam", "Cập nhật thông tin thành công!", NotificationType.Information);
                yield return new TestCaseData("", "0987682438", "bao113@gmail.com", "052205003846", "11/02/2005", "Nữ", "Họ tên không được để trống!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "", "052205003846", "11/02/2005", "Khác", "Email không được để trống!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "ngocvo2502", "052205003846", "11/02/2005", "Nữ", "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "ngocvo2502@", "052205003846", "11/02/2005", "Nam", "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "ngocvo2502@gmail", "052205003846", "11/02/2005", "Nữ", "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "ngocvo2502@.com", "052205003846", "11/02/2005", "Khác", "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "@gmail.com", "052205003846", "11/02/2005", "Nữ", "Email không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "yry12333@gmail.com", "052205003846", "11/02/2005", "Nam", "Email đã được sử dụng bởi tài khoản khác!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "098768243", "bao113@gmail.com", "052205003846", "11/02/2005", "Khác", "Số điện thoại không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "098768243a", "bao113@gmail.com", "052205003846", "11/02/2005", "Nữ", "Số điện thoại không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "", "bao113@gmail.com", "052205003846", "11/02/2005", "Nam", "Cập nhật thông tin thành công!", NotificationType.Information);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "05220500384a", "11/02/2005", "Nữ", "Số căn cước không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "05220500384", "11/02/2005", "Nữ", "Số căn cước không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "", "11/02/2005", "Khác", "Cập nhật thông tin thành công!", NotificationType.Information);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "052205003846", "11/02/2026", "Nam", "Ngày sinh không hợp lệ!", NotificationType.Warning);
                yield return new TestCaseData("Bui Quoc Bao", "0987682438", "bao113@gmail.com", "", "", "Nam", "Cập nhật thông tin thành công!", NotificationType.Information);
            }
        }

        [Test]
        [TestCaseSource(nameof(SaveProfileTestCases))]
        public async Task SaveProfile_Test(string name, string phone, string email, string cccd, string dob, string gender, string expectedMessage, NotificationType expectedType)
        {
            _viewModel.EditHoTen = name;
            _viewModel.EditSoDienThoai = phone;
            _viewModel.EditEmail = email;
            _viewModel.EditCanCuoc = cccd;
            _viewModel.EditGioiTinh = gender;
            
            if (DateTime.TryParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDob))
                _viewModel.EditNgaySinh = parsedDob;

            await _viewModel.SaveProfileCommand.ExecuteAsync(null);

            if (expectedType == NotificationType.Information)
            {
                _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            }
            else
            {
                _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
            }

            _mockNotificationService.Verify(n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains(expectedMessage)), expectedType, It.IsAny<bool>()), Times.Once);
        }

    }
}