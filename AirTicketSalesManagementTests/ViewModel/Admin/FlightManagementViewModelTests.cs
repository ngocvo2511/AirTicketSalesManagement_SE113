using Moq;
using AirTicketSalesManagement.ViewModel.Admin;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.ViewModel;

namespace AirTicketSalesManagementTests.ViewModel.Admin
{
    [TestFixture]
    public class FlightManagementViewModelTests
    {
        [TestFixture]
        public class SaveAddFlightTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private FlightManagementViewModel _viewModel;
            private Mock<AirTicketDbContext> _dbContextMock;

            private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
            {
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

                // Setup DbSets
                var chuyenbays = new List<Chuyenbay>().AsQueryable();
                var sanbays = new List<Sanbay>
            {
                new Sanbay { MaSb = "SGN", ThanhPho = "HCM", QuocGia = "Việt Nam" },
                new Sanbay { MaSb = "HAN", ThanhPho = "HN", QuocGia = "Việt Nam" },
                new Sanbay { MaSb = "DAD", ThanhPho = "Đà Nẵng", QuocGia = "Việt Nam" }

            }.AsQueryable();
                var quydinhs = new List<Quydinh>
            {
                new Quydinh { SoSanBayTgtoiDa = 2, TgdungMin = 10, TgdungMax = 30 }
            }.AsQueryable();

                var chuyenbayDbSet = CreateMockDbSet(chuyenbays);
                var sanbayDbSet = CreateMockDbSet(sanbays);
                var quydinhDbSet = CreateMockDbSet(quydinhs);

                _dbContextMock.Setup(x => x.Chuyenbays).Returns(chuyenbayDbSet.Object);
                _dbContextMock.Setup(x => x.Sanbays).Returns(sanbayDbSet.Object);
                _dbContextMock.Setup(x => x.Quydinhs).Returns(quydinhDbSet.Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                _viewModel = new FlightManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object)
                {
                    SanBayList = new ObservableCollection<string>
                {
                    "HCM (SGN), VN",
                    "HN (HAN), VN",
                    "Đà Nẵng (DAD), VN"
                }
                };
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_AddDiemDi_IsNull()
            {
                _viewModel.AddDiemDi = null;
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = "Đang khai thác";

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_AddDiemDen_IsNull()
            {
                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = null;
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = "Đang khai thác";

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_AddSoHieuCB_IsNull()
            {
                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = null;
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = "Đang khai thác";

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_AddHangHangKhong_IsNull()
            {
                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddHangHangKhong = null;
                _viewModel.AddTTKhaiThac = "Đang khai thác";

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_AddTTKhaiThac_IsNull()
            {
                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = null;

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldWarn_When_FlightCode_IsDuplicate()
            {
                var existedFlight = new List<Chuyenbay>
            {
                new Chuyenbay { SoHieuCb = "VN123" }
            }.AsQueryable();

                var existedSet = CreateMockDbSet(existedFlight);
                _dbContextMock.Setup(x => x.Chuyenbays).Returns(existedSet.Object);

                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = "VN123"; // trùng
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = "Đang khai thác";

                _viewModel.SaveFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Số hiệu chuyến bay đã tồn tại. Vui lòng nhập số hiệu khác.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveFlight_ShouldAddFlight_When_Valid()
            {
                _viewModel.AddDiemDi = "Đà Nẵng (DAD), Việt Nam";
                _viewModel.AddDiemDen = "Hà Nội (HAN), Việt Nam";
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddHangHangKhong = "Vietnam Airlines";
                _viewModel.AddTTKhaiThac = "Đang khai thác";
                _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();

                _viewModel.SaveFlight();

                _dbContextMock.Verify(x => x.Chuyenbays.Add(It.Is<Chuyenbay>(cb =>
                    cb.SoHieuCb == "VN123" &&
                    cb.SbdiNavigation.MaSb == "DAD" &&
                    cb.SbdenNavigation.MaSb == "HAN" &&
                    cb.HangHangKhong == "Vietnam Airlines" &&
                    cb.TtkhaiThac == "Đang khai thác")), Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Chuyến bay đã được thêm thành công")),
                        NotificationType.Information,
                        false),
                    Times.Once);
            }
        }

        [TestFixture]
        public class SaveEditFlightTests
        {
        }
    }
}