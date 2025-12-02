using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using AirTicketSalesManagement.ViewModel.Admin;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.ViewModel;

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
