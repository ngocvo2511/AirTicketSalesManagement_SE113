using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface; // Thêm namespace chứa IEmailService nếu cần
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.EmailServices; // Namespace chứa EmailTemplateService
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Services.Session;
using AirTicketSalesManagement.ViewModel.Customer;
using AirTicketSalesManagement.ViewModel; // Chứa NotificationType
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class CancelTicket_test
    {
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IUserSessionService> _mockUserSessionService;
        private Mock<EmailTemplateService> _mockTemplateService;
        private Mock<AirTicketDbContext> _mockContext;
        
        private BookingHistoryViewModel _viewModel;
        private KQLichSuDatVe _bookingStub;
        private Datve _datVeStub; // Đổi Phieudat -> Datve
        private Lichbay _lichBayStub;
        private List<Ctdv> _ctdvListStub;

        [SetUp]
        public void Setup()
        {
            _lichBayStub = new Lichbay { MaLb = 1, SoHieuCb = "VN123", GioDi = DateTime.Now.AddDays(5) };
            
            // Đổi Phieudat -> Datve
            _datVeStub = new Datve 
            { 
                MaDv = 1, 
                MaLb = 1, 
                MaLbNavigation = _lichBayStub, 
                TtdatVe = "Đã thanh toán", 
                Email = "user@example.com" 
            };
            
            _ctdvListStub = new List<Ctdv> { new Ctdv { MaDv = 1, MaHvLb = 10 }, new Ctdv { MaDv = 1, MaHvLb = 10 } };

            _bookingStub = new KQLichSuDatVe
            {
                MaVe = 1,
                TrangThai = "Đã thanh toán",
                GioDi = DateTime.Now.AddDays(5),
                QdHuyVe = 1 // Giả lập dữ liệu để CanCancel tính ra true
            };

            _mockContext = new Mock<AirTicketDbContext>();
            
            // Đổi DbSet<Phieudat> -> DbSet<Datve>
            _mockContext.Setup(c => c.Datves).Returns(CreateMockDbSet(new List<Datve> { _datVeStub }).Object);
            
            _mockContext.Setup(c => c.Ctdvs).Returns(CreateMockDbSet(_ctdvListStub).Object);
            _mockContext.Setup(c => c.Hangvetheolichbays).Returns(CreateMockDbSet(new List<Hangvetheolichbay> { new Hangvetheolichbay { MaHvLb = 10, SlveConLai = 5 } }).Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _mockDbContextService = new Mock<IAirTicketDbContextService>();
            _mockDbContextService.Setup(s => s.CreateDbContext()).Returns(_mockContext.Object);
            
            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), false)).ReturnsAsync(true);
            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), true)).ReturnsAsync(true); 
            
            _mockEmailService = new Mock<IEmailService>();
            _mockTemplateService = new Mock<EmailTemplateService>();
            _mockUserSessionService = new Mock<IUserSessionService>();

            _viewModel = new BookingHistoryViewModel(
                1, new CustomerViewModel(), _mockEmailService.Object, _mockTemplateService.Object,
                _mockDbContextService.Object, null, _mockUserSessionService.Object, _mockNotificationService.Object);
        }

        public static IEnumerable<TestCaseData> CancelTicketTestCases
        {
            get
            {
                yield return new TestCaseData("Đã thanh toán", true, true, "Hủy vé thành công.");
                yield return new TestCaseData("Đã thanh toán", false, true, "Vé không thể hủy vì đã quá thời hạn hủy vé.");
                yield return new TestCaseData("Đã hủy", true, true, "Vé đã được hủy trước đó.");
                yield return new TestCaseData("Đã thanh toán", true, true, "Không tìm thấy vé để hủy."); 
                yield return new TestCaseData("Đã thanh toán", true, true, "Lỗi khi hủy vé:"); 
                yield return new TestCaseData("Chưa thanh toán (Online)", false, true, "Vé không thể hủy vì đã quá thời hạn hủy vé.");
                yield return new TestCaseData("Chưa thanh toán (Tiền mặt)", true, true, "Hủy vé thành công.");
            }
        }

        [Test]
        [TestCaseSource(nameof(CancelTicketTestCases))]
        public async Task CancelTicket_Test(string status, bool canCancel, bool confirmationResult, string expectedErrorMessage)
        {
            // Update Stub Data để CanCancel tính toán đúng (thay vì gán trực tiếp nếu property là readonly)
            _bookingStub.TrangThai = status;
            
            if (canCancel)
            {
                _bookingStub.QdHuyVe = 1; // 1 ngày
                _bookingStub.GioDi = DateTime.Now.AddDays(5); // Còn xa mới bay -> Được hủy
            }
            else
            {
                _bookingStub.QdHuyVe = 1;
                _bookingStub.GioDi = DateTime.Now.AddHours(1); // Sắp bay -> Không được hủy
            }

            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Information, true)).ReturnsAsync(confirmationResult);
            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, true)).ReturnsAsync(confirmationResult);

            if (status != "Đã hủy" && canCancel && expectedErrorMessage?.Contains("Lỗi khi hủy vé:") == true)
            {
                _mockContext.Setup(c => c.SaveChangesAsync(default)).ThrowsAsync(new Exception("DB Error"));
            }
            
            if (expectedErrorMessage?.Contains("Không tìm thấy vé") == true)
            {
                // Chỉ định rõ kiểu Datve cho FirstOrDefaultAsync
                _mockContext.Setup(c => c.Datves.FirstOrDefaultAsync(It.IsAny<Expression<Func<Datve, bool>>>(), default)).ReturnsAsync((Datve)null);
            }

            await _viewModel.CancelTicketCommand.ExecuteAsync(_bookingStub);

            if (expectedErrorMessage != null && !expectedErrorMessage.Contains("Lỗi khi hủy vé") && !expectedErrorMessage.Contains("Không tìm thấy vé"))
            {
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains(expectedErrorMessage)), NotificationType.Warning, false), 
                    Times.Once);
                _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
            }
            else if (expectedErrorMessage?.Contains("Lỗi khi hủy vé") == true)
            {
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Lỗi khi hủy vé")), NotificationType.Error, false), 
                    Times.Once);
            }
            else if (expectedErrorMessage?.Contains("Không tìm thấy vé") == true)
            {
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Không tìm thấy vé")), NotificationType.Error, false), 
                    Times.Once);
            }
            else if (confirmationResult == true)
            {
                _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(It.Is<string>(msg => msg.Contains("Hủy vé thành công.")), NotificationType.Information, false), 
                    Times.Once);
            }
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            // Fix CS0411 bằng cách dùng It.IsAny với Expression cụ thể
            mockSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<T, bool>>>(), default))
                   .ReturnsAsync((Expression<Func<T, bool>> p, CancellationToken t) => queryable.FirstOrDefault(p.Compile()));
                   
            mockSet.Setup(d => d.Where(It.IsAny<Expression<Func<T, bool>>>()))
                   .Returns<Expression<Func<T, bool>>>(predicate => mockSet.Object.AsQueryable().Where(predicate));
                   
            return mockSet;
        }
    }
}