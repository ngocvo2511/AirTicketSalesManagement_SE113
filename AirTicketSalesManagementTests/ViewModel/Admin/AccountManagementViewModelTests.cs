using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AirTicketSalesManagement.ViewModel.Admin;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services;

namespace AirTicketSalesManagementTests.ViewModel.Admin
{
    [TestFixture]
    public class AccountManagementViewModelTests
    {
        [TestFixture]
        public class SaveAddAccountTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private AccountManagementViewModel _viewModel;

            private List<Taikhoan> _taikhoans;
            private List<Nhanvien> _nhanviens;
            private List<Khachhang> _khachhangs;

            private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> backingList) where T : class
            {
                var queryable = backingList.AsQueryable();
                var mockSet = new Mock<DbSet<T>>();

                mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
                mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
                mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
                mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

                mockSet.Setup(d => d.Add(It.IsAny<T>()))
                       .Callback<T>(entity => backingList.Add(entity));

                return mockSet;
            }

            [SetUp]
            public void SetUp()
            {
                _dbContextServiceMock = new Mock<IAirTicketDbContextService>();
                _notificationServiceMock = new Mock<INotificationService>();
                _dbContextMock = new Mock<AirTicketDbContext>();

                _taikhoans = new List<Taikhoan>();
                _nhanviens = new List<Nhanvien>();
                _khachhangs = new List<Khachhang>();

                var taikhoanDbSet = CreateMockDbSet(_taikhoans);
                var nhanvienDbSet = CreateMockDbSet(_nhanviens);
                var khachhangDbSet = CreateMockDbSet(_khachhangs);

                // Auto-assign IDs when adding staff/customers
                nhanvienDbSet.Setup(d => d.Add(It.IsAny<Nhanvien>()))
                    .Callback<Nhanvien>(nv =>
                    {
                        if (nv.MaNv == 0) nv.MaNv = _nhanviens.Count + 1;
                        _nhanviens.Add(nv);
                    });

                khachhangDbSet.Setup(d => d.Add(It.IsAny<Khachhang>()))
                    .Callback<Khachhang>(kh =>
                    {
                        if (kh.MaKh == 0) kh.MaKh = _khachhangs.Count + 1;
                        _khachhangs.Add(kh);
                    });

                _dbContextMock.Setup(x => x.Taikhoans).Returns(taikhoanDbSet.Object);
                _dbContextMock.Setup(x => x.Nhanviens).Returns(nhanvienDbSet.Object);
                _dbContextMock.Setup(x => x.Khachhangs).Returns(khachhangDbSet.Object);

                _dbContextMock.Setup(x => x.SaveChanges()).Returns(1);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                _viewModel = new AccountManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object)
                {
                    UserList = new ObservableCollection<UserSelectionModel>()
                };
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenMissingEmailFields()
            {
                _viewModel.AddEmail = "";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin tài khoản")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenMissingRoleFields()
            {
                _viewModel.AddEmail = "newuser@mail.com";
                _viewModel.AddRole = string.Empty;
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin tài khoản")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenMissingPasswordFields()
            {
                _viewModel.AddEmail = "newuser@mail.com";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin tài khoản")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenMissingFullNameFields()
            {
                _viewModel.AddEmail = "newuser@mail.com";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin tài khoản")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenInvalidEmail()
            {
                _viewModel.AddEmail = "test@";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Email không hợp lệ")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddAccount_ShouldWarn_WhenEmailDuplicated()
            {
                _taikhoans.Add(new Taikhoan { MaTk = 1, Email = "yry12333@gmail.com", VaiTro = "Admin" });

                _viewModel.AddEmail = "yry12333@gmail.com";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Email này đã được sử dụng")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.Nhanviens.Add(It.IsAny<Nhanvien>()), Times.Never);
                _dbContextMock.Verify(x => x.Khachhangs.Add(It.IsAny<Khachhang>()), Times.Never);
                _dbContextMock.Verify(x => x.Taikhoans.Add(It.IsAny<Taikhoan>()), Times.Never);
            }

            [Test]
            public async Task SaveAddAccount_ShouldAddAccount()
            {
                _viewModel.AddEmail = "newuser@mail.com";
                _viewModel.AddRole = "Admin";
                _viewModel.AddPassword = "P@ssw0rd123";
                _viewModel.AddFullName = "Ngoc Vo";

                await Task.Run(() => _viewModel.SaveAddAccount());

                _dbContextMock.Verify(x => x.Nhanviens.Add(It.Is<Nhanvien>(nv => nv.HoTenNv == "Ngoc Vo")), Times.Once);

                _dbContextMock.Verify(x => x.Taikhoans.Add(It.Is<Taikhoan>(tk =>
                    tk.Email == "newuser@mail.com" &&
                    tk.VaiTro == "Admin" &&
                    tk.MaNv.HasValue &&
                    !tk.MaKh.HasValue &&
                    !string.IsNullOrWhiteSpace(tk.MatKhau) &&
                    tk.MatKhau != "P@ssw0rd!")), Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.AtLeast(2));

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Tài khoản đã được thêm thành công")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                Assert.That(_viewModel.IsAddPopupOpen, Is.False);
            }           
        }

        [TestFixture]
        public class SearchTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private AccountManagementViewModel _viewModel;
            private List<Taikhoan> _taikhoans;

            private Mock<DbSet<Taikhoan>> CreateMockDbSet(List<Taikhoan> data)
            {
                var queryable = data.AsQueryable();
                var mockSet = new Mock<DbSet<Taikhoan>>();
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.Provider).Returns(queryable.Provider);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.Expression).Returns(queryable.Expression);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
                return mockSet;
            }

            private Mock<DbSet<T>> CreateEmptyMockDbSet<T>() where T : class
            {
                var data = new List<T>().AsQueryable();
                var mockSet = new Mock<DbSet<T>>();
                mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
                mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
                mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
                mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
                return mockSet;
            }


            [SetUp]
            public void SetUp()
            {
                _dbContextServiceMock = new Mock<IAirTicketDbContextService>();
                _notificationServiceMock = new Mock<INotificationService>();
                _dbContextMock = new Mock<AirTicketDbContext>();

                _taikhoans = new List<Taikhoan>
                {
                    new Taikhoan { MaTk = 1, Email = "admin@mail.com", VaiTro = "Admin", MaNv = 1, MaKh = null, MatKhau = "pw1" },
                    new Taikhoan { MaTk = 2, Email = "staff@mail.com", VaiTro = "Nhân viên", MaNv = 2, MaKh = null, MatKhau = "pw2" },
                    new Taikhoan { MaTk = 3, Email = "customer@mail.com", VaiTro = "Khách hàng", MaNv = null, MaKh = 1, MatKhau = "pw3" }
                };

                var taikhoanDbSet = CreateMockDbSet(_taikhoans);
                _dbContextMock.Setup(x => x.Taikhoans).Returns(taikhoanDbSet.Object);
                _dbContextMock.Setup(x => x.Nhanviens).Returns(CreateEmptyMockDbSet<Nhanvien>().Object);
                _dbContextMock.Setup(x => x.Khachhangs).Returns(CreateEmptyMockDbSet<Khachhang>().Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _viewModel = new AccountManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object);
            }

            [Test]
            public void Search_ByEmail_FiltersAccounts()
            {
                _viewModel.SearchEmail = "admin";
                _viewModel.SearchRole = string.Empty;

                _viewModel.Search();

                Assert.AreEqual(1, _viewModel.Accounts.Count);
                Assert.That(_viewModel.Accounts[0].Email, Is.EqualTo("admin@mail.com"));
            }

            [Test]
            public void Search_ByRole_FiltersAccounts()
            {
                _viewModel.SearchEmail = string.Empty;
                _viewModel.SearchRole = "Nhân viên";

                _viewModel.Search();

                Assert.AreEqual(1, _viewModel.Accounts.Count);
                Assert.That(_viewModel.Accounts[0].VaiTro, Is.EqualTo("Nhân viên"));
            }

            [Test]
            public void Search_ByEmailAndRole_FiltersAccounts()
            {
                _viewModel.SearchEmail = "customer";
                _viewModel.SearchRole = "Khách hàng";

                _viewModel.Search();

                Assert.AreEqual(1, _viewModel.Accounts.Count);
                Assert.That(_viewModel.Accounts[0].Email, Is.EqualTo("customer@mail.com"));
                Assert.That(_viewModel.Accounts[0].VaiTro, Is.EqualTo("Khách hàng"));
            }

            [Test]
            public void Search_NoMatch_ReturnsEmpty()
            {
                _viewModel.SearchEmail = "notfound";
                _viewModel.SearchRole = "Admin";

                _viewModel.Search();

                Assert.AreEqual(0, _viewModel.Accounts.Count);
            }

            [Test]
            public void Search_RoleIsTatCa_DoesNotFilterByRole()
            {
                _viewModel.SearchEmail = string.Empty;
                _viewModel.SearchRole = "Tất cả";

                _viewModel.Search();

                Assert.AreEqual(3, _viewModel.Accounts.Count);
            }
        }

        [TestFixture]
        public class DeleteAccountTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private AccountManagementViewModel _viewModel;
            private List<Taikhoan> _taikhoans;

            private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
            {
                var queryable = data.AsQueryable();
                var mockSet = new Mock<DbSet<T>>();
                mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
                mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
                mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
                mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
                return mockSet;
            }

            private Mock<DbSet<Taikhoan>> CreateMockDbSet(List<Taikhoan> data)
            {
                var queryable = data.AsQueryable();
                var mockSet = new Mock<DbSet<Taikhoan>>();
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.Provider).Returns(queryable.Provider);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.Expression).Returns(queryable.Expression);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
                mockSet.As<IQueryable<Taikhoan>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
                mockSet.Setup(m => m.Remove(It.IsAny<Taikhoan>())).Callback<Taikhoan>(tk => data.Remove(tk));
                return mockSet;
            }

            [SetUp]
            public void SetUp()
            {
                _dbContextServiceMock = new Mock<IAirTicketDbContextService>();
                _notificationServiceMock = new Mock<INotificationService>();
                _dbContextMock = new Mock<AirTicketDbContext>();

                _taikhoans = new List<Taikhoan>
        {
            new Taikhoan { MaTk = 1, Email = "admin@mail.com", VaiTro = "Admin", MaNv = 1, MaKh = null, MatKhau = "pw1" },
            new Taikhoan { MaTk = 2, Email = "staff@mail.com", VaiTro = "Nhân viên", MaNv = 2, MaKh = null, MatKhau = "pw2" }
        };

                var taikhoanDbSet = CreateMockDbSet(_taikhoans);
                _dbContextMock.Setup(x => x.Taikhoans).Returns(taikhoanDbSet.Object);
                _dbContextMock.Setup(x => x.SaveChanges()).Returns(1);

                // Setup empty DbSet for Nhanviens and Khachhangs to avoid null
                var nhanviens = new List<Nhanvien>(); // hoặc có thể thêm dữ liệu mẫu nếu muốn
                var khachhangs = new List<Khachhang>();

                _dbContextMock.Setup(x => x.Nhanviens).Returns(CreateMockDbSet(nhanviens).Object);
                _dbContextMock.Setup(x => x.Khachhangs).Returns(CreateMockDbSet(khachhangs).Object);


                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync((string msg, NotificationType type, bool isConfirm) =>
                    {
                        // Nếu là xác nhận xóa, giả lập luôn đồng ý
                        if (isConfirm) return true;
                        return true;
                    });

                _viewModel = new AccountManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object);
            }

            [Test]
            public async Task DeleteAccount_ShouldWarn_When_SelectedAccount_IsNull()
            {
                _viewModel.SelectedAccount = null;

                await Task.Run(() => _viewModel.DeleteAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng chọn một tài khoản để xóa")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task DeleteAccount_ShouldWarn_When_AccountNotFound()
            {
                _viewModel.SelectedAccount = new AccountModel { Email = "notfound@mail.com" };

                await Task.Run(() => _viewModel.DeleteAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Không tìm thấy tài khoản trong cơ sở dữ liệu")),
                        NotificationType.Error,
                        false),
                    Times.Once);
            }


            [Test]
            public async Task DeleteAccount_ShouldDelete_When_Valid()
            {
                _viewModel.SelectedAccount = new AccountModel { Email = "staff@mail.com" };

                await Task.Run(() => _viewModel.DeleteAccount());

                // Đã xóa khỏi danh sách
                Assert.IsFalse(_taikhoans.Any(tk => tk.Email == "staff@mail.com"));

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Đã xóa tài khoản thành công")),
                        NotificationType.Information,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task DeleteAccount_ShouldShowError_When_Exception()
            {
                _viewModel.SelectedAccount = new AccountModel { Email = "staff@mail.com" };

                // Gây lỗi khi SaveChanges
                _dbContextMock.Setup(x => x.SaveChanges()).Throws(new Exception("DB error"));

                await Task.Run(() => _viewModel.DeleteAccount());

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Đã xảy ra lỗi khi xóa tài khoản")),
                        NotificationType.Error,
                        false),
                    Times.Once);
            }
        }

    }
}