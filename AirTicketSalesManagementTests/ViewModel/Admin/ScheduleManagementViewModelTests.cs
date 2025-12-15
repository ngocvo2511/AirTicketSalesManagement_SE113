using Moq;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public class ScheduleManagementViewModelTests
    {
        [TestFixture]
        public class SaveAddScheduleTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private ScheduleManagementViewModel _viewModel;

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

                // Default data for DbSets used by constructor and SaveAddSchedule
                var sanbays = new List<Sanbay>().AsQueryable();
                var chuyenbays = new List<Chuyenbay>().AsQueryable(); // used in ctor LoadSoHieuCB
                var lichbays = new List<Lichbay>().AsQueryable(); // used in ctor LoadFlightSchedule
                var quydinhs = new List<Quydinh>
                {
                    new Quydinh { ThoiGianBayToiThieu = 30, SoHangVe = 2 }
                }.AsQueryable();
                var hangves = new List<Hangve>
                {
                    new Hangve { MaHv = 1, TenHv = "Phổ thông" },
                    new Hangve { MaHv = 2, TenHv = "Thương gia" }
                }.AsQueryable();
                var sanbaytrunggians = new List<Sanbaytrunggian>().AsQueryable();
                var hangveTheoLichBays = new List<Hangvetheolichbay>().AsQueryable();

                // Mock sets
                var sanbayDbSet = CreateMockDbSet(sanbays);
                var chuyenbayDbSet = CreateMockDbSet(chuyenbays);
                var lichbayDbSet = CreateMockDbSet(lichbays);
                var quydinhDbSet = CreateMockDbSet(quydinhs);
                var hangveDbSet = CreateMockDbSet(hangves);
                var sbtgDbSet = CreateMockDbSet(sanbaytrunggians);
                var hvTlbDbSet = CreateMockDbSet(hangveTheoLichBays);

                // Setup context to return sets
                _dbContextMock.Setup(x => x.Sanbays).Returns(sanbayDbSet.Object);
                _dbContextMock.Setup(x => x.Chuyenbays).Returns(chuyenbayDbSet.Object);
                _dbContextMock.Setup(x => x.Lichbays).Returns(lichbayDbSet.Object);
                _dbContextMock.Setup(x => x.Quydinhs).Returns(quydinhDbSet.Object);
                _dbContextMock.Setup(x => x.Hangves).Returns(hangveDbSet.Object);
                _dbContextMock.Setup(x => x.Sanbaytrunggians).Returns(sbtgDbSet.Object);
                _dbContextMock.Setup(x => x.Hangvetheolichbays).Returns(hvTlbDbSet.Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                _viewModel = new ScheduleManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object)
                {
                    TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>()
                };
            }

            private void PrepareValidInput(
                DateTime start,
                DateTime end,
                string departTime = "08:30",
                string duration = "02:00",
                int totalSeats = 100,
                decimal baseFare = 1_500_000m)
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = start;
                _viewModel.AddDenNgay = end;
                _viewModel.AddGioDi = departTime;
                _viewModel.AddThoiGianBay = duration;
                _viewModel.AddLoaiMB = "A321";
                _viewModel.AddSLVeKT = totalSeats.ToString(CultureInfo.InvariantCulture);
                _viewModel.AddGiaVe = baseFare.ToString(CultureInfo.InvariantCulture);
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "60", SLVeConLai = "60" },
                    new HangVeTheoLichBay { TenHangVe = "Thương gia", SLVeToiDa = "40", SLVeConLai = "40" }
                };
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_SoHieuCBFieldsMissing()
            {
                _viewModel.AddSoHieuCB = null;
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddGioDiFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldError_When_TimeFormatInvalid_AddGioDi()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "as";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Định dạng giờ không hợp lệ")),
                        NotificationType.Error,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddThoiGianBayFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldError_When_TimeFormatInvalid_AddThoiGianBay()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "abc";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Định dạng giờ không hợp lệ")),
                        NotificationType.Error,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddTuNgayFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = null;
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddDenNgayFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = null;
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddLoaiMBFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = string.Empty;
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddSLVeKTFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_InvalidNumberFormat_SLVeKT()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "abc";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Số lượng vé khai thác không hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_NegativeNumber_AddSLVeKT()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "-1";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Số lượng vé khai thác không hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddGiaVeFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_InvalidNumberFormat_AddGiaVe()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "abc";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Giá vé không hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_AddTTLichBayFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = string.Empty;
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }



            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_EndDateBeforeStartDate()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 16);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Ngày kết thúc không được nhỏ hơn ngày bắt đầu")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_TenHangVeFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = string.Empty,  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập đầy đủ thông tin hạng ghế.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_SLveToiDaFieldsMissing()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập đầy đủ thông tin hạng ghế.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_SLveToiDaInvalidFormat()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "abc", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập giá vé của hạng ghế hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_ShouldWarn_When_SLveToiDaMisMatchSLVeKT()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "99", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Tổng số lượng vé của các hạng ghế phải bằng số lượng vé khai thác.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);
            }

            [Test]
            public async Task SaveAddSchedule_Success()
            {
                _viewModel.AddSoHieuCB = "VN123";
                _viewModel.AddTuNgay = new DateTime(2026, 1, 17);
                _viewModel.AddDenNgay = new DateTime(2026, 1, 17);
                _viewModel.AddGioDi = "08:00";
                _viewModel.AddThoiGianBay = "02:15";
                _viewModel.AddLoaiMB = "Airbus A321";
                _viewModel.AddSLVeKT = "100";
                _viewModel.AddGiaVe = "1000000";
                _viewModel.AddTTLichBay = "Chờ cất cánh";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveAddSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Đã thêm 1 lịch bay thành công!")),
                        NotificationType.Information,
                        false),
                    Times.Once);
            }
        }
        [TestFixture]
        public class SaveEditScheduleTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock; private Mock<INotificationService> _notificationServiceMock; private Mock<AirTicketDbContext> _dbContextMock; private ScheduleManagementViewModel _viewModel;
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

                // Minimal data used by SaveEditSchedule
                var sanbays = new List<Sanbay>().AsQueryable();
                var chuyenbays = new List<Chuyenbay>().AsQueryable();
                var lichbays = new List<Lichbay>
                {
                    new Lichbay
                    {
                        MaLb = 1,
                        SoHieuCb = "VN123",
                        GioDi = new DateTime(2025,12,31,8,0,0),
                        GioDen = new DateTime(2025,12,31,9,30,0),
                        LoaiMb = "A321",
                        TtlichBay = "Chờ cất cánh",
                        SlveKt = 100,
                        GiaVe = 1000000
                    }
                }.AsQueryable();
                            var quydinhs = new List<Quydinh>
                {
                    new Quydinh { ThoiGianBayToiThieu = 30, SoHangVe = 1 }
                }.AsQueryable();
                            var hangves = new List<Hangve>
                {
                    new Hangve { MaHv = 1, TenHv = "Phổ thông" },
                }.AsQueryable();
                var hangveTheoLichBays = new List<Hangvetheolichbay>().AsQueryable();
                var sanbaytrunggians = new List<Sanbaytrunggian>().AsQueryable();

                // Mock sets
                var sanbayDbSet = CreateMockDbSet(sanbays);
                var chuyenbayDbSet = CreateMockDbSet(chuyenbays);
                var lichbayDbSet = CreateMockDbSet(lichbays);
                var quydinhDbSet = CreateMockDbSet(quydinhs);
                var hangveDbSet = CreateMockDbSet(hangves);
                var hvTlbDbSet = CreateMockDbSet(hangveTheoLichBays);
                var sbtgDbSet = CreateMockDbSet(sanbaytrunggians);

                _dbContextMock.Setup(x => x.Sanbays).Returns(sanbayDbSet.Object);
                _dbContextMock.Setup(x => x.Chuyenbays).Returns(chuyenbayDbSet.Object);
                _dbContextMock.Setup(x => x.Lichbays).Returns(lichbayDbSet.Object);
                _dbContextMock.Setup(x => x.Quydinhs).Returns(quydinhDbSet.Object);
                _dbContextMock.Setup(x => x.Hangves).Returns(hangveDbSet.Object);
                _dbContextMock.Setup(x => x.Hangvetheolichbays).Returns(hvTlbDbSet.Object);
                _dbContextMock.Setup(x => x.Sanbaytrunggians).Returns(sbtgDbSet.Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _notificationServiceMock
                    .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                _viewModel = new ScheduleManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object);
                _viewModel.IsEditSchedulePopupOpen = true;

                _viewModel.EditID = 1;
                _viewModel.EditSoHieuCB = "VN123";
                _viewModel.EditNgayDi = new DateTime(2026, 1, 17);
                _viewModel.EditNgayDen = new DateTime(2026, 1, 17);
                _viewModel.EditGioDi = "08:00";
                _viewModel.EditGioDen = "10:15";
                _viewModel.EditLoaiMB = "Airbus A321";
                _viewModel.EditSLVeKT = "100";
                _viewModel.EditGiaVe = "1000000";
                _viewModel.EditTTLichBay = "Chờ cất cánh";
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditGioDiFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditGioDi = "";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldError_When_TimeFormatInvalid_EditGioDi()
            {
                _viewModel.EditGioDi = "as";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Định dạng giờ không hợp lệ")),
                        NotificationType.Error,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditGioDenFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditGioDen = "";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldError_When_TimeFormatInvalid_EditGioDen()
            {
                _viewModel.EditGioDen = "abc";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Định dạng giờ không hợp lệ")),
                        NotificationType.Error,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditNgayDiFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditNgayDi = null;

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditNgayDenFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditNgayDen = null;

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditLoaiMBFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditLoaiMB = string.Empty;

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditSLVeKTFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditSLVeKT = "";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_InvalidNumberFormat_SLVeKT()
            {
                _viewModel.EditSLVeKT = "abc";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Số lượng vé khai thác không hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_NegativeNumber_SLVeKT()
            {
                _viewModel.EditSLVeKT = "-1";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Số lượng vé khai thác không hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditGiaVeFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditGiaVe = "";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_InvalidNumberFormat_GiaVe()
            {
                // Make a required field invalid
                _viewModel.EditGiaVe = "abc";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Giá vé không hợp lệ")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EditTTLichBayFieldsMissing()
            {
                // Make a required field invalid
                _viewModel.EditTTLichBay = string.Empty;

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng điền đầy đủ thông tin lịch bay")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EndDateBeforeStartDate()
            {
                _viewModel.EditNgayDi = new DateTime(2026, 1, 17);
                _viewModel.EditNgayDen = new DateTime(2026, 1, 16);

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Thời gian đến phải sau thời gian đi.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_EndTimeBeforeStartTime_SameDate()
            {
                _viewModel.EditGioDi = "08:00";
                _viewModel.EditGioDen = "07:00";

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Thời gian đến phải sau thời gian đi.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_TenHangVeFieldsMissing()
            {
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = string.Empty,  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập đầy đủ thông tin hạng ghế.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_SLVeToiDaFieldsMissing()
            {
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = string.Empty,  SLVeToiDa = "", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập đầy đủ thông tin hạng ghế.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_InvalidNumberFormat_SLVeToiDa()
            {
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "abc", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Vui lòng nhập giá vé của hạng ghế hợp lệ.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_ShouldWarn_When_MismatchSLVeToiDa_SLVeKT()
            {
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "99", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Tổng số lượng vé của các hạng ghế phải bằng số lượng vé khai thác.")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.True);
            }

            [Test]
            public async Task SaveEditSchedule_SuccessChangeSoHieuCB_ShouldPersist_AndNotify()
            {
                _viewModel.EditSoHieuCB = "QH301";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Once);

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Lịch bay đã được cập nhật thành công!")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.False);
            }

            [Test]
            public async Task SaveEditSchedule_SuccessChangeLoaiMB_ShouldPersist_AndNotify()
            {
                _viewModel.EditLoaiMB = "Boeing 737";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Once);

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Lịch bay đã được cập nhật thành công!")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.False);
            }

            [Test]
            public async Task SaveEditSchedule_SuccessChangeGiaVe_ShouldPersist_AndNotify()
            {
                _viewModel.EditGiaVe = "1200000";
                _viewModel.TicketClassForScheduleList = new ObservableCollection<HangVeTheoLichBay>
                {
                    new HangVeTheoLichBay { TenHangVe = "Phổ thông",  SLVeToiDa = "100", SLVeConLai = "100" },
                };

                await _viewModel.SaveEditSchedule();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Once);

                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Lịch bay đã được cập nhật thành công!")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                Assert.That(_viewModel.IsEditSchedulePopupOpen, Is.False);
            }
        }
    }
}