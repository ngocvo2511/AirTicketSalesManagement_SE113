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
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private FlightManagementViewModel _viewModel;
            private Mock<AirTicketDbContext> _dbContextMock;

            private Chuyenbay _selectedFlight;

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

                var sanbays = new List<Sanbay>
                {
                    new Sanbay { MaSb = "CXR", ThanhPho = "Khánh Hòa", QuocGia = "Việt Nam" },
                    new Sanbay { MaSb = "HUI", ThanhPho = "Thừa Thiên - Huế", QuocGia = "Việt Nam" },
                    new Sanbay { MaSb = "HAN", ThanhPho = "Hà Nội", QuocGia = "Việt Nam" }

                }.AsQueryable();

                var quydinhs = new List<Quydinh>
                {
                    new Quydinh { TgdungMin = 10, TgdungMax = 30 }
                }.AsQueryable();

                var sanbayDbSet = CreateMockDbSet(sanbays);
                var quydinhDbSet = CreateMockDbSet(quydinhs);

                _dbContextMock.Setup(x => x.Sanbays).Returns(sanbayDbSet.Object);
                _dbContextMock.Setup(x => x.Quydinhs).Returns(quydinhDbSet.Object);

                _selectedFlight = new Chuyenbay
                {
                    SoHieuCb = "VJ206",
                    SbdiNavigation = sanbays.First(sb => sb.MaSb == "CXR"),
                    SbdenNavigation = sanbays.First(sb => sb.MaSb == "HUI"),
                    HangHangKhong = "Vietjet Air",
                    TtkhaiThac = "Đang khai thác",
                    Sanbaytrunggians = new List<Sanbaytrunggian>(),
                    Lichbays = new List<Lichbay>()
                };

                var chuyenbays = new List<Chuyenbay> { _selectedFlight }.AsQueryable();
                var chuyenbayDbSet = CreateMockDbSet(chuyenbays);

                _dbContextMock.Setup(x => x.Chuyenbays).Returns(chuyenbayDbSet.Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                _viewModel = new FlightManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object)
                {
                    SanBayList = new ObservableCollection<string>
                    {
                        "Khánh Hòa (CXR), Việt Nam",
                        "Thừa Thiên - Huế (HUI), Việt Nam",
                        "Hà Nội (HAN), Việt Nam"

                    },
                    SelectedFlight = _selectedFlight
                };
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_EditDiemDi_IsNull()
            {
                _viewModel.EditDiemDi = null;
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_EditDiemDen_IsNull()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = null;
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_EditSoHieuCB_IsNull()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = null;
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_EditHangHangKhong_IsNull()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = null;
                _viewModel.EditTTKhaiThac = "Đang khai thác";

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_EditTTKhaiThac_IsNull()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = null;

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin chuyến bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_ThoiGianDung_LessThanMin()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";
                _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>
                {
                    new SBTG { STT = 1, MaSBTG = "Hà Nội (HAN), Việt Nam", ThoiGianDung = 5 }
                };


                // Setup RemoveRange and Add for Sanbaytrunggians
                _dbContextMock.Setup(x => x.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()));
                _dbContextMock.Setup(x => x.Sanbaytrunggians.Add(It.IsAny<Sanbaytrunggian>()));

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Thời gian dừng tối thiểu là: 10 phút")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_ThoiGianDung_GreaterThanMax()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";
                _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>
                {
                    new SBTG { STT = 1, MaSBTG = "Hà Nội (HAN), Việt Nam", ThoiGianDung = 40 }
                };
                // Setup RemoveRange and Add for Sanbaytrunggians
                _dbContextMock.Setup(x => x.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()));
                _dbContextMock.Setup(x => x.Sanbaytrunggians.Add(It.IsAny<Sanbaytrunggian>()));

                // Setup SaveChanges
                _dbContextMock.Setup(x => x.SaveChanges());

                // Setup RemoveRange and Add for Sanbaytrunggians
                _dbContextMock.Setup(x => x.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()));
                _dbContextMock.Setup(x => x.Sanbaytrunggians.Add(It.IsAny<Sanbaytrunggian>()));

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Thời gian dừng tối đa là: 30 phút")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public void SaveEditFlight_ShouldUpdateFlight_When_Valid()
            {
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";
                _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();

                // Setup RemoveRange and Add for Sanbaytrunggians
                _dbContextMock.Setup(x => x.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()));
                _dbContextMock.Setup(x => x.Sanbaytrunggians.Add(It.IsAny<Sanbaytrunggian>()));

                // Setup SaveChanges
                _dbContextMock.Setup(x => x.SaveChanges());

                _viewModel.SaveEditFlight();

                // Verify that SaveChanges was called
                _dbContextMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);

                // Verify notification
                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Chuyến bay đã được cập nhật thành công")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                // Verify popup closed
                Assert.IsFalse(_viewModel.IsEditPopupOpen);
            }

            [Test]
            public void SaveEditFlight_ShouldWarn_When_SelectedFlight_IsNull()
            {
                _viewModel.SelectedFlight = null;
                _viewModel.EditDiemDi = "Khánh Hòa (CXR), Việt Nam";
                _viewModel.EditDiemDen = "Thừa Thiên - Huế (HUI), Việt Nam";
                _viewModel.EditSoHieuCB = "VJ206";
                _viewModel.EditHangHangKhong = "Vietjet Air";
                _viewModel.EditTTKhaiThac = "Đang khai thác";

                _viewModel.SaveEditFlight();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Đã xảy ra lỗi khi cập nhật chuyến bay: ")),
                        NotificationType.Error,
                        false),
                    Times.Once);
            }
        }

        [TestFixture]
        public class DeleteFlightTests
        {
            private Mock<IAirTicketDbContextService> _mockDbContextService;
            private Mock<INotificationService> _mockNotificationService;
            private Mock<AirTicketDbContext> _mockDbContext;
            private FlightManagementViewModel _viewModel;

            [SetUp]
            public void Setup()
            {
                _mockDbContextService = new Mock<IAirTicketDbContextService>();
                _mockNotificationService = new Mock<INotificationService>();
                _mockDbContext = new Mock<AirTicketDbContext>();

                // Mock Sanbays
                var sanbaySet = new List<Sanbay>().AsQueryable().BuildMockDbSet();
                _mockDbContext.Setup(c => c.Sanbays).Returns(sanbaySet.Object);

                // Mock Chuyenbays với navigation property không null
                var chuyenbayList = new List<Chuyenbay>
                {
                    new Chuyenbay
                    {
                        SoHieuCb = "VN123",
                        SbdiNavigation = new Sanbay { MaSb = "SGN" },
                        SbdenNavigation = new Sanbay { MaSb = "HAN" },
                        Sanbaytrunggians = new List<Sanbaytrunggian>(),
                        Lichbays = new List<Lichbay>()
                    }
                }.AsQueryable();
                var chuyenbaySet = chuyenbayList.BuildMockDbSet();
                _mockDbContext.Setup(c => c.Chuyenbays).Returns(chuyenbaySet.Object);

                // Mock Sanbaytrunggians
                var sbtgSet = new List<Sanbaytrunggian>().AsQueryable().BuildMockDbSet();
                _mockDbContext.Setup(c => c.Sanbaytrunggians).Returns(sbtgSet.Object);

                // Mock Lichbays
                var lichbaySet = new List<Lichbay>().AsQueryable().BuildMockDbSet();
                _mockDbContext.Setup(c => c.Lichbays).Returns(lichbaySet.Object);

                _mockDbContextService.Setup(s => s.CreateDbContext()).Returns(_mockDbContext.Object);

                _viewModel = new FlightManagementViewModel(_mockDbContextService.Object, _mockNotificationService.Object);
            }


            [Test]
            public async Task DeleteFlight_NoSelectedFlight_ShouldShowWarning()
            {
                _viewModel.SelectedFlight = null;
                _mockNotificationService
                    .Setup(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, false))
                    .ReturnsAsync(false);

                await Task.Run(() => _viewModel.DeleteFlight());

                _mockNotificationService.Verify(
                    s => s.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Vui lòng chọn một chuyến bay")), NotificationType.Warning, false),
                    Times.Once);
            }

            [Test]
            public async Task DeleteFlight_UserCancelsConfirmation_ShouldNotDelete()
            {
                var flight = new Chuyenbay { SoHieuCb = "VN123", Lichbays = new List<Lichbay>(), Sanbaytrunggians = new List<Sanbaytrunggian>() };
                _viewModel.SelectedFlight = flight;

                var chuyenbaySet = new List<Chuyenbay> { flight }.AsQueryable().BuildMockDbSet();
                var lichbaySet = new List<Lichbay>().AsQueryable().BuildMockDbSet();
                var sbtgSet = new List<Sanbaytrunggian>().AsQueryable().BuildMockDbSet();

                _mockDbContext.Setup(c => c.Chuyenbays).Returns(chuyenbaySet.Object);
                _mockDbContext.Setup(c => c.Lichbays).Returns(lichbaySet.Object);
                _mockDbContext.Setup(c => c.Sanbaytrunggians).Returns(sbtgSet.Object);

                _mockNotificationService
                    .Setup(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, true))
                    .ReturnsAsync(false);

                await Task.Run(() => _viewModel.DeleteFlight());

                _mockDbContext.Verify(c => c.Chuyenbays.Remove(It.IsAny<Chuyenbay>()), Times.Never);
                _mockDbContext.Verify(c => c.SaveChanges(), Times.Never);
            }

            [Test]
            public async Task DeleteFlight_FlightNotFound_ShouldShowError()
            {
                var flight = new Chuyenbay { SoHieuCb = "VN123" };
                _viewModel.SelectedFlight = flight;

                var chuyenbaySet = new List<Chuyenbay>().AsQueryable().BuildMockDbSet();
                var lichbaySet = new List<Lichbay>().AsQueryable().BuildMockDbSet();
                var sbtgSet = new List<Sanbaytrunggian>().AsQueryable().BuildMockDbSet();

                _mockDbContext.Setup(c => c.Chuyenbays).Returns(chuyenbaySet.Object);
                _mockDbContext.Setup(c => c.Lichbays).Returns(lichbaySet.Object);
                _mockDbContext.Setup(c => c.Sanbaytrunggians).Returns(sbtgSet.Object);

                _mockNotificationService
                    .SetupSequence(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, true))
                    .ReturnsAsync(true);

                _mockNotificationService
                    .Setup(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Error, false))
                    .ReturnsAsync(false);

                await Task.Run(() => _viewModel.DeleteFlight());

                _mockNotificationService.Verify(
                    s => s.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Không tìm thấy chuyến bay")), NotificationType.Error, false),
                    Times.Once);
            }

            [Test]
            public async Task DeleteFlight_FlightHasLichbay_ShouldShowWarning()
            {
                var flight = new Chuyenbay
                {
                    SoHieuCb = "VN123",
                    Lichbays = new List<Lichbay> { new Lichbay() },
                    Sanbaytrunggians = new List<Sanbaytrunggian>()
                };
                _viewModel.SelectedFlight = flight;

                var chuyenbaySet = new List<Chuyenbay> { flight }.AsQueryable().BuildMockDbSet();
                var lichbaySet = new List<Lichbay> { new Lichbay() }.AsQueryable().BuildMockDbSet();
                var sbtgSet = new List<Sanbaytrunggian>().AsQueryable().BuildMockDbSet();

                _mockDbContext.Setup(c => c.Chuyenbays).Returns(chuyenbaySet.Object);
                _mockDbContext.Setup(c => c.Lichbays).Returns(lichbaySet.Object);
                _mockDbContext.Setup(c => c.Sanbaytrunggians).Returns(sbtgSet.Object);

                _mockNotificationService
                    .SetupSequence(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, true))
                    .ReturnsAsync(true);

                _mockNotificationService
                    .Setup(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, false))
                    .ReturnsAsync(false);

                await Task.Run(() => _viewModel.DeleteFlight());

                _mockNotificationService.Verify(
                    s => s.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Không thể xóa chuyến bay đã có lịch bay")), NotificationType.Warning, false),
                    Times.Once);
            }

            [Test]
            public async Task DeleteFlight_FlightWithoutLichbay_ShouldDeleteAndShowSuccess()
            {
                var flight = new Chuyenbay
                {
                    SoHieuCb = "VN123",
                    Lichbays = new List<Lichbay>(),
                    Sanbaytrunggians = new List<Sanbaytrunggian> { new Sanbaytrunggian() }
                };
                _viewModel.SelectedFlight = flight;

                var chuyenbaySet = new List<Chuyenbay> { flight }.AsQueryable().BuildMockDbSet();
                var lichbaySet = new List<Lichbay>().AsQueryable().BuildMockDbSet();
                var sbtgSet = new List<Sanbaytrunggian> { new Sanbaytrunggian() }.AsQueryable().BuildMockDbSet();

                _mockDbContext.Setup(c => c.Chuyenbays).Returns(chuyenbaySet.Object);
                _mockDbContext.Setup(c => c.Lichbays).Returns(lichbaySet.Object);
                _mockDbContext.Setup(c => c.Sanbaytrunggians).Returns(sbtgSet.Object);

                _mockNotificationService
                    .SetupSequence(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, true))
                    .ReturnsAsync(true);

                _mockNotificationService
                    .Setup(s => s.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Information, false))
                    .ReturnsAsync(false);

                _mockDbContext.Setup(c => c.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()));
                _mockDbContext.Setup(c => c.Chuyenbays.Remove(It.IsAny<Chuyenbay>()));
                _mockDbContext.Setup(c => c.SaveChanges());

                await Task.Run(() => _viewModel.DeleteFlight());

                _mockDbContext.Verify(c => c.Sanbaytrunggians.RemoveRange(It.IsAny<IEnumerable<Sanbaytrunggian>>()), Times.Once);
                _mockDbContext.Verify(c => c.Chuyenbays.Remove(It.IsAny<Chuyenbay>()), Times.Once);
                _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
                _mockNotificationService.Verify(
                    s => s.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Đã xóa chuyến bay thành công")), NotificationType.Information, false),
                    Times.Once);
            }
        }



        //    [TestFixture]
        //    public class AddIntermediateAirportTests
        //    {
        //        // Mock Dependencies
        //        private Mock<IAirTicketDbContextService> _mockDbContextService;
        //        private Mock<INotificationService> _mockNotificationService;
        //        private Mock<AirTicketDbContext> _mockContext;

        //        // System Under Test
        //        private FlightManagementViewModel _viewModel;

        //        [SetUp]
        //        public void Setup()
        //        {
        //            // 1. Setup Mock DbContext & Services
        //            _mockContext = new Mock<AirTicketDbContext>();

        //            _mockDbContextService = new Mock<IAirTicketDbContextService>();
        //            _mockDbContextService.Setup(s => s.CreateDbContext())
        //                                 .Returns(_mockContext.Object);

        //            _mockNotificationService = new Mock<INotificationService>();
        //            // Setup mặc định trả về true để code không bị lỗi khi await
        //            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
        //                                    .ReturnsAsync(true);

        //            // 2. Setup dữ liệu giả cơ bản (tránh NullReference khi khởi tạo ViewModel)
        //            var emptySanbay = new List<Sanbay>();
        //            var emptyChuyenbay = new List<Chuyenbay>();

        //            _mockContext.Setup(c => c.Sanbays).Returns(CreateMockDbSet(emptySanbay).Object);
        //            _mockContext.Setup(c => c.Chuyenbays).Returns(CreateMockDbSet(emptyChuyenbay).Object);

        //            // 3. Khởi tạo ViewModel
        //            _viewModel = new FlightManagementViewModel(
        //                _mockDbContextService.Object,
        //                _mockNotificationService.Object
        //            );
        //        }

        //        [Test]
        //        public void AddIntermediateAirport_CountBelowLimit_Success()
        //        {
        //            // Arrange
        //            int initialCount = 2;
        //            int maxLimit = 3;

        //            var listQuyDinh = new List<Quydinh>
        //{
        //    new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit }
        //};
        //            _mockContext.Setup(c => c.Quydinhs)
        //                        .Returns(CreateMockDbSet(listQuyDinh).Object);

        //            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();
        //            for (int i = 0; i < initialCount; i++)
        //            {
        //                _viewModel.DanhSachSBTG.Add(new SBTG { STT = i + 1 });
        //            }

        //            // Act
        //            Assert.DoesNotThrow(() => _viewModel.AddIntermediateAirport());

        //            // Assert
        //            Assert.That(_viewModel.DanhSachSBTG.Count, Is.EqualTo(initialCount + 1));
        //            _mockNotificationService.Verify(
        //                n => n.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, It.IsAny<bool>()),
        //                Times.Never
        //            );
        //        }

        //        [Test]
        //        public void AddIntermediateAirport_CountEqualsLimit_ShowWarning()
        //        {
        //            // Arrange
        //            int initialCount = 3;
        //            int maxLimit = 3;

        //            var listQuyDinh = new List<Quydinh>
        //{
        //    new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit }
        //};
        //            _mockContext.Setup(c => c.Quydinhs)
        //                        .Returns(CreateMockDbSet(listQuyDinh).Object);

        //            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();
        //            for (int i = 0; i < initialCount; i++)
        //            {
        //                _viewModel.DanhSachSBTG.Add(new SBTG { STT = i + 1 });
        //            }

        //            // Act
        //            Assert.DoesNotThrow(() => _viewModel.AddIntermediateAirport());

        //            // Assert
        //            Assert.That(_viewModel.DanhSachSBTG.Count, Is.EqualTo(initialCount));
        //            _mockNotificationService.Verify(
        //                n => n.ShowNotificationAsync(
        //                    It.Is<string>(msg => msg.Contains("Số sân bay trung gian tối đa là: 3")),
        //                    NotificationType.Warning,
        //                    It.IsAny<bool>()
        //                ),
        //                Times.Once
        //            );
        //        }


        //        //Truong hop khong co quy dinh trong DB
        //        [Test]
        //        public void AddIntermediateAirport_NoQuyDinh_ShouldShowWarning()
        //        {
        //            // Arrange: Không có quy định trong DB
        //            _mockContext.Setup(c => c.Quydinhs).Returns(CreateMockDbSet(new List<Quydinh>()).Object);

        //            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();

        //            // Act
        //            _viewModel.AddIntermediateAirport();

        //            // Assert: Không thêm mới, cảnh báo tối đa là 0
        //            Assert.AreEqual(0, _viewModel.DanhSachSBTG.Count, "Không được thêm khi không có quy định.");
        //            _mockNotificationService.Verify(
        //                n => n.ShowNotificationAsync(
        //                    It.Is<string>(msg => msg.Contains("Số sân bay trung gian tối đa là: 0")),
        //                    NotificationType.Warning,
        //                    It.IsAny<bool>()),
        //                Times.Once,
        //                "Phải hiển thị cảnh báo khi không có quy định."
        //            );
        //        }

        //        //Truong hop so san bay trung gian hien tai vuot qua gioi han
        //        [Test]
        //        public void AddIntermediateAirport_CountAboveLimit_ShouldShowWarning()
        //        {
        //            // Arrange: Số lượng hiện tại > giới hạn
        //            int initialCount = 5;
        //            int maxLimit = 3;
        //            var listQuyDinh = new List<Quydinh> { new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit } };
        //            _mockContext.Setup(c => c.Quydinhs).Returns(CreateMockDbSet(listQuyDinh).Object);

        //            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();
        //            for (int i = 0; i < initialCount; i++)
        //            {
        //                _viewModel.DanhSachSBTG.Add(new SBTG { STT = i + 1 });
        //            }

        //            // Act
        //            _viewModel.AddIntermediateAirport();

        //            // Assert: Không thêm mới, cảnh báo đúng
        //            Assert.AreEqual(initialCount, _viewModel.DanhSachSBTG.Count, "Không được thêm khi đã vượt giới hạn.");
        //            _mockNotificationService.Verify(
        //                n => n.ShowNotificationAsync(
        //                    It.Is<string>(msg => msg.Contains($"Số sân bay trung gian tối đa là: {maxLimit}")),
        //                    NotificationType.Warning,
        //                    It.IsAny<bool>()),
        //                Times.Once,
        //                "Phải hiển thị cảnh báo khi đã vượt giới hạn."
        //            );
        //        }

        //        //Truong hop them moi thanh cong, kiem tra thuoc tinh SBTG moi
        //        [Test]
        //        public void AddIntermediateAirport_NewSBTG_ShouldHaveCorrectProperties()
        //        {
        //            // Arrange: Cho phép thêm mới
        //            int initialCount = 0;
        //            int maxLimit = 2;
        //            var listQuyDinh = new List<Quydinh> { new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit } };
        //            _mockContext.Setup(c => c.Quydinhs).Returns(CreateMockDbSet(listQuyDinh).Object);

        //            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();

        //            // Act
        //            _viewModel.AddIntermediateAirport();

        //            // Assert: Kiểm tra thuộc tính của SBTG mới
        //            Assert.AreEqual(1, _viewModel.DanhSachSBTG.Count, "Phải thêm mới SBTG.");
        //            var sbtg = _viewModel.DanhSachSBTG.First();
        //            Assert.AreEqual(1, sbtg.STT, "STT phải là 1.");
        //            Assert.AreEqual(string.Empty, sbtg.MaSBTG, "MaSBTG phải rỗng.");
        //            Assert.AreEqual(0, sbtg.ThoiGianDung, "ThoiGianDung phải là 0.");
        //            Assert.AreEqual(string.Empty, sbtg.GhiChu, "GhiChu phải rỗng.");
        //            Assert.IsNotNull(sbtg.SbtgList, "SbtgList phải được khởi tạo.");
        //        }


        //        //Truong hop DanhSachSBTG = null
        //        [Test]
        //        public void AddIntermediateAirport_DanhSachSBTGNull_ShouldNotThrow()
        //        {
        //            // Arrange: DanhSachSBTG = null
        //            int maxLimit = 2;
        //            var listQuyDinh = new List<Quydinh> { new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit } };
        //            _mockContext.Setup(c => c.Quydinhs).Returns(CreateMockDbSet(listQuyDinh).Object);

        //            _viewModel.DanhSachSBTG = null;

        //            // Act & Assert: Không bị lỗi, không thêm mới
        //            Assert.DoesNotThrow(() => _viewModel.AddIntermediateAirport(), "Không được throw exception khi danh sách null.");
        //        }


        //        // --- HELPER: Mock DbSet (Cần thiết cho EF Core Mocking) ---
        //        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        //        {
        //            var queryable = sourceList.AsQueryable();
        //            var mockSet = new Mock<DbSet<T>>();

        //            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        //            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        //            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        //            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        //            return mockSet;
        //        }
        //    }
    }
    // Helper extension for mocking DbSet<T>
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}