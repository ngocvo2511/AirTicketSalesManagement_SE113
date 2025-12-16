using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
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
    public class SaveInfantAge_test
    {
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<AirTicketDbContext> _mockContext;
        private RegulationManagementViewModel _viewModel;
        private Quydinh _quyDinhStub;

        [SetUp]
        public void Setup()
        {
            _quyDinhStub = new Quydinh
            {
                Id = 1,
                TuoiToiDaSoSinh = 2,
                TuoiToiDaTreEm = 12
            };

            var listQuyDinh = new List<Quydinh>
    {
        _quyDinhStub
    };

            _mockContext = new Mock<AirTicketDbContext>();

            // ✅ DbSet async-safe
            _mockContext.Setup(c => c.Quydinhs)
                .ReturnsDbSet(listQuyDinh);

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

            _viewModel = new RegulationManagementViewModel(
                _mockDbContextService.Object,
                _mockNotificationService.Object);

            _viewModel.InfantAge = 2;
            _viewModel.ChildAge = 12;
        }


        public static IEnumerable<TestCaseData> SaveInfantAgeTestCases
        {
            get
            {
                yield return new TestCaseData(5, 12, "Lưu thành công.", NotificationType.Information);
                yield return new TestCaseData(13, 12, "Tuổi tối đa của trẻ sơ sinh phải nhỏ hơn tuổi tối đa của trẻ em.", NotificationType.Warning);
            }
        }

        [Test]
        [TestCaseSource(nameof(SaveInfantAgeTestCases))]
        public async Task SaveInfantAge_Test(int newInfantAge, int currentChildAge, string expectedMessage, NotificationType expectedType)
        {
            _viewModel.EditInfantAge = newInfantAge;
            _viewModel.ChildAge = currentChildAge;
            _quyDinhStub.TuoiToiDaTreEm = currentChildAge;

            await _viewModel.SaveInfantAgeCommand.ExecuteAsync(null);

            _mockNotificationService.Verify(n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains(expectedMessage)), expectedType, It.IsAny<bool>()), Times.Once);
            
            if(expectedType == NotificationType.Information)
                _mockContext.Verify(c => c.SaveChanges(), Times.Once);
            else
                _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }
    }
}