using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Interface;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.EmailServices;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.Services.Session;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Staff;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests.ViewModel.Staff
{
    [TestFixture]
    public class ConfirmPayment_test
    {
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IEmailTemplateService> _mockTemplateService;
        private Mock<AirTicketDbContext> _mockContext;

        private TicketManagementViewModel _viewModel;
        private QuanLiDatVe _bookingVmStub;
        private Datve _datVeStub;
        private Lichbay _lichBayStub;

        [SetUp]
        public void Setup()
        {
            // Lịch bay
            _lichBayStub = new Lichbay
            {
                MaLb = 1,
                SoHieuCb = "VN123",
                GioDi = DateTime.Now.AddDays(5)
            };

            // Bản ghi đặt vé
            _datVeStub = new Datve
            {
                MaDv = 1,
                MaLb = 1,
                MaLbNavigation = _lichBayStub,
                TtdatVe = "Chưa thanh toán (Tiền mặt)",
                Email = "user@example.com",
                TongTienTt = 2_000_000m
            };

            // View-model ticket
            _bookingVmStub = new QuanLiDatVe
            {
                MaVe = 1,
                TrangThai = "Chưa thanh toán (Tiền mặt)",
                GioDi = _lichBayStub.GioDi,
                QdDatVe = 1,
                NgayDat = DateTime.Now.AddDays(-1)
            };

            _mockContext = new Mock<AirTicketDbContext>();

            // DbSet Datves
            _mockContext.Setup(c => c.Datves)
                .ReturnsDbSet(new List<Datve> { _datVeStub });

            _mockContext.Setup(c => c.Lichbays)
                .ReturnsDbSet(new List<Lichbay> { _lichBayStub });

            _mockContext.Setup(c => c.SaveChangesAsync(default))
                .ReturnsAsync(1);

            _mockDbContextService = new Mock<IAirTicketDbContextService>();
            _mockDbContextService
                .Setup(s => s.CreateDbContext())
                .Returns(_mockContext.Object);

            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService
                .Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                .ReturnsAsync(true); // mặc định đồng ý / OK

            _mockEmailService = new Mock<IEmailService>();
            _mockTemplateService = new Mock<IEmailTemplateService>();

            _mockTemplateService
                .Setup(t => t.BuildBookingSuccess(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<decimal>()))
                .Returns("EMAIL_BODY");

            UserSession.Current.Email = "session@example.com";

            var parent = new BaseViewModel();

            _viewModel = new TicketManagementViewModel(
                parent,
                _mockDbContextService.Object,
                _mockNotificationService.Object,
                _mockEmailService.Object,
                _mockTemplateService.Object
            );
        }

        // ------------------------------
        // ve == null
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldWarn_When_VeIsNull()
        {
            await _viewModel.ConfirmPayment(null);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Vé không hợp lệ.")),
                    NotificationType.Warning,
                    false),
                Times.Once);

            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        // ------------------------------
        // CanConfirm == false
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldWarn_When_CanConfirmIsFalse()
        {
            _bookingVmStub.QdDatVe = 1;
            _bookingVmStub.GioDi = DateTime.Now.AddHours(12); // quá hạn xác nhận

            await _viewModel.ConfirmPayment(_bookingVmStub);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Không thể xác nhận thanh toán vé này do đã quá thời hạn đặt vé.")),
                    NotificationType.Warning,
                    false),
                Times.Once);

            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }


        // ------------------------------
        // Trạng thái không phải "Chưa thanh toán (Tiền mặt)"
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldWarn_When_StatusNotCashUnpaid()
        {
            _bookingVmStub.TrangThai = "Đã thanh toán";

            await _viewModel.ConfirmPayment(_bookingVmStub);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Không thể xác nhận thanh toán.")),
                    NotificationType.Warning,
                    false),
                Times.Once);

            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
        }

        // ------------------------------
        // Người dùng bấm No ở popup xác nhận
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldNotProceed_When_UserCancelsConfirmation()
        {
            _mockNotificationService
                .Setup(n => n.ShowNotificationAsync(
                    It.IsAny<string>(),
                    NotificationType.Information,
                    true))
                .ReturnsAsync(false); // user chọn No

            await _viewModel.ConfirmPayment(_bookingVmStub);

            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Xác nhận thanh toán thành công.")),
                    NotificationType.Information,
                    false),
                Times.Never);
            _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // ------------------------------
        // Không tìm thấy Datve trong DB
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldError_When_DatveNotFound()
        {
            _mockContext.Setup(c => c.Datves)
                .ReturnsDbSet(new List<Datve>()); // rỗng

            await _viewModel.ConfirmPayment(_bookingVmStub);

            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Không tìm thấy vé để xác nhận thanh toán.")),
                    NotificationType.Error,
                    false),
                Times.Once);
        }

        // ------------------------------
        // Thành công + gửi email
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_Success_ShouldUpdateStatus_SaveAndSendEmail()
        {
            _mockNotificationService
                .Setup(n => n.ShowNotificationAsync(
                    It.Is<string>(s => s.Contains("Bạn có chắc chắn")),
                    NotificationType.Information,
                    true))
                .ReturnsAsync(true);


            await _viewModel.ConfirmPayment(_bookingVmStub);

            // DB save
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);

            // Datve trong context được cập nhật
            Assert.That(_datVeStub.TtdatVe, Is.EqualTo("Đã thanh toán"));

            // VM cập nhật trạng thái
            Assert.That(_bookingVmStub.TrangThai, Is.EqualTo("Đã thanh toán"));

            // Thông báo thành công
            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Xác nhận thanh toán thành công.")),
                    NotificationType.Information,
                    false),
                Times.Once);

            // Gửi email
            _mockTemplateService.Verify(
                t => t.BuildBookingSuccess(
                    It.Is<string>(s => s == "VN123"),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.Is<decimal>(p => p == 2_000_000m)),
                Times.Once);

            _mockEmailService.Verify(
                e => e.SendEmailAsync(
                    It.Is<string>(to => to == "user@example.com"),
                    It.Is<string>(sub => sub.Contains("Thanh toán vé chuyến bay VN123")),
                    "EMAIL_BODY"),
                Times.Once);
        }

        // ------------------------------
        // Lỗi khi SaveChangesAsync
        // ------------------------------
        [Test]
        public async Task ConfirmPayment_ShouldError_When_SaveChangesThrows()
        {
            _mockContext.Setup(c => c.SaveChangesAsync(default))
                .ThrowsAsync(new Exception("DB Error"));

            await _viewModel.ConfirmPayment(_bookingVmStub);

            _mockNotificationService.Verify(
                n => n.ShowNotificationAsync(
                    It.Is<string>(msg => msg.Contains("Lỗi khi xác nhận thanh toán")),
                    NotificationType.Error,
                    false),
                Times.Once);
        }
    }
}