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
    }
}