using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests.ViewModel.Admin
{
    [TestFixture]
    public class RegulationManagementViewModelTests
    {
        [TestFixture]
        public class SaveChildAgeTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private RegulationManagementViewModel _vm;

            // Ensure DbSet supports async (IAsyncEnumerable) and LINQ provider for EF Core
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
            public async Task SetUp()
            {
                _dbContextServiceMock = new Mock<IAirTicketDbContextService>();
                _notificationServiceMock = new Mock<INotificationService>();
                _dbContextMock = new Mock<AirTicketDbContext>();

                _notificationServiceMock
                    .Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                // Provide an existing regulation for LoadRegulationAsync()
                var initialRegList = new List<Quydinh>
                {
                    new Quydinh
                    {
                        TuoiToiDaSoSinh = 2,   // InfantAge
                        TuoiToiDaTreEm  = 12   // ChildAge
                    }
                };

                var quyDinhDbSet = CreateMockDbSet(initialRegList.AsQueryable());

                _dbContextMock.Setup(x => x.Quydinhs).Returns(quyDinhDbSet.Object);
                _dbContextMock.Setup(x => x.SaveChanges()).Returns(1);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                // Create VM and explicitly load regulation (avoid Task.Delay race)
                _vm = new RegulationManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object);
                _vm.LoadRegulation();

                // Sanity: the VM should now reflect the seeded regulation
                Assert.That(_vm.InfantAge, Is.EqualTo(2));
                Assert.That(_vm.ChildAge,  Is.EqualTo(12));
            }

            // -------------------------------------------------------------
            // CASE 1: EditChildAge <= InfantAge → Warning
            // -------------------------------------------------------------
            [Test]
            public async Task SaveChildAge_EditLessOrEqualInfantAge_ShouldWarn_AndNotSave()
            {
                // Arrange
                _vm.EditChildAge = 1;
                _vm.IsEditingChildAge = true;

                // Act
                await _vm.SaveChildAge();

                // Assert
                _notificationServiceMock.Verify(x =>
                    x.ShowNotificationAsync(
                        It.Is<string>(msg =>
                            msg.Contains("phải lớn hơn tuổi tối đa của trẻ sơ sinh")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_vm.IsEditingChildAge, Is.True);
                Assert.That(_vm.ChildAge, Is.EqualTo(12));
            }

            // -------------------------------------------------------------
            // CASE 2: EditChildAge > InfantAge → Save OK
            // -------------------------------------------------------------
            [Test]
            public async Task SaveChildAge_ValidGreaterThanInfantAge_ShouldUpdateAndPersist()
            {
                // Arrange
                _vm.EditChildAge = 6;
                _vm.IsEditingChildAge = true;

                // Act
                await _vm.SaveChildAge();

                // Assert
                _notificationServiceMock.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("Lưu thành công")),
                        NotificationType.Information,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Once);

                Assert.That(_vm.ChildAge, Is.EqualTo(6));
                Assert.That(_vm.IsEditingChildAge, Is.False);
            }
        }

        [TestFixture]
        public class SaveMaxAirportsTests
        {
            private Mock<IAirTicketDbContextService> _dbContextServiceMock;
            private Mock<INotificationService> _notificationServiceMock;
            private Mock<AirTicketDbContext> _dbContextMock;
            private RegulationManagementViewModel _vm;

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

                _notificationServiceMock
                    .Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                    .ReturnsAsync(true);

                // Provide an existing regulation for LoadRegulation
                var initialRegList = new List<Quydinh>
                {
                    new Quydinh { SoSanBay = 10 }
                };
                var quyDinhDbSet = CreateMockDbSet(initialRegList.AsQueryable());
                _dbContextMock.Setup(x => x.Quydinhs).Returns(quyDinhDbSet.Object);
                _dbContextMock.Setup(x => x.SaveChanges()).Returns(1);

                // Default: 0 sân bay
                var sanbayDbSet = CreateMockDbSet(new List<Sanbay>().AsQueryable());
                _dbContextMock.Setup(x => x.Sanbays).Returns(sanbayDbSet.Object);

                _dbContextServiceMock.Setup(x => x.CreateDbContext()).Returns(_dbContextMock.Object);

                _vm = new RegulationManagementViewModel(_dbContextServiceMock.Object, _notificationServiceMock.Object);
                _vm.LoadRegulation();

                Assert.That(_vm.MaxAirports, Is.EqualTo(10));
            }

            // CASE 1: Không hợp lệ (giá trị âm) => Không lưu
            [Test]
            public async Task SaveMaxAirports_EditNegative_ShouldNotSave()
            {
                _vm.EditMaxAirports = -1;
                _vm.IsEditingMaxAirports = true;

                await _vm.SaveMaxAirports();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_vm.MaxAirports, Is.EqualTo(10));
                Assert.That(_vm.IsEditingMaxAirports, Is.True);
            }

            // CASE 2: Không thay đổi giá trị => Không lưu
            [Test]
            public async Task SaveMaxAirports_NoChange_ShouldNotSave()
            {
                _vm.EditMaxAirports = 10;
                _vm.MaxAirports = 10;
                _vm.IsEditingMaxAirports = true;

                await _vm.SaveMaxAirports();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_vm.IsEditingMaxAirports, Is.False);
            }

            // CASE 3: Số sân bay hiện tại lớn hơn giới hạn mới => Cảnh báo, không lưu
            [Test]
            public async Task SaveMaxAirports_TooManyAirports_ShouldWarn_AndNotSave()
            {
                // Có 5 sân bay, giới hạn mới là 3
                _dbContextMock
                .Setup(x => x.Sanbays)
                .ReturnsDbSet(new List<Sanbay>
                {
                    new Sanbay { MaSb = "A" },
                    new Sanbay { MaSb = "B" },
                    new Sanbay { MaSb = "C" },
                    new Sanbay { MaSb = "D" },
                    new Sanbay { MaSb = "E" }
                });

                _vm.EditMaxAirports = 3;
                _vm.MaxAirports = 10;
                _vm.IsEditingMaxAirports = true;

                await _vm.SaveMaxAirports();

                _notificationServiceMock.Verify(x =>
                    x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains("lớn hơn giới hạn mới")),
                        NotificationType.Warning,
                        false),
                    Times.Once);

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Never);
                Assert.That(_vm.MaxAirports, Is.EqualTo(10));
                Assert.That(_vm.IsEditingMaxAirports, Is.True);
            }

            // CASE 4: Lưu thành công khi hợp lệ
            [Test]
            public async Task SaveMaxAirports_Valid_ShouldUpdateAndPersist()
            {
                _dbContextMock
                .Setup(x => x.Sanbays)
                .ReturnsDbSet(new List<Sanbay>
                {
                    new Sanbay { MaSb = "A" },
                    new Sanbay { MaSb = "B" },
                    new Sanbay { MaSb = "C" },
                    new Sanbay { MaSb = "D" },
                    new Sanbay { MaSb = "E" }
                });
                _dbContextMock.Setup(x => x.Quydinhs)
                .ReturnsDbSet(new List<Quydinh>
                {
                    new Quydinh { Id = 1, SoSanBay = 10 }
                });

                _vm.EditMaxAirports = 6;
                _vm.MaxAirports = 10;
                _vm.IsEditingMaxAirports = true;

                await _vm.SaveMaxAirports();

                _dbContextMock.Verify(x => x.SaveChanges(), Times.Once);
                
                Assert.That(_vm.MaxAirports, Is.EqualTo(6));
                Assert.That(_vm.IsEditingMaxAirports, Is.False);
            }
        }
    }
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
