using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Admin;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class AddIntermediateAirport_test
    {
        // Mock Dependencies
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private Mock<INotificationService> _mockNotificationService;
        private Mock<AirTicketDbContext> _mockContext;

        // System Under Test
        private FlightManagementViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // 1. Setup Mock DbContext & Services
            _mockContext = new Mock<AirTicketDbContext>();

            _mockDbContextService = new Mock<IAirTicketDbContextService>();
            _mockDbContextService.Setup(s => s.CreateDbContext())
                                 .Returns(_mockContext.Object);

            _mockNotificationService = new Mock<INotificationService>();
            // Setup mặc định trả về true để code không bị lỗi khi await
            _mockNotificationService.Setup(n => n.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                                    .ReturnsAsync(true);

            // 2. Setup dữ liệu giả cơ bản (tránh NullReference khi khởi tạo ViewModel)
            var emptySanbay = new List<Sanbay>();
            var emptyChuyenbay = new List<Chuyenbay>();

            _mockContext.Setup(c => c.Sanbays).Returns(CreateMockDbSet(emptySanbay).Object);
            _mockContext.Setup(c => c.Chuyenbays).Returns(CreateMockDbSet(emptyChuyenbay).Object);

            // 3. Khởi tạo ViewModel
            _viewModel = new FlightManagementViewModel(
                _mockDbContextService.Object,
                _mockNotificationService.Object
            );
        }

        // --- TEST DATA ---
        public static IEnumerable<TestCaseData> AirportTestCases
        {
            get
            {
                // UTCID01: Normal Case (Thêm thành công)
                // Input: Hiện có 2 sân bay, Quy định max 3. 
                // Expect: Không có lỗi (null), số lượng tăng lên 3.
                yield return new TestCaseData(2, 3, null)
                    .SetName("UTCID01_CountBelowLimit_Success");

                // UTCID02: Abnormal/Boundary Case (Bị chặn)
                // Input: Hiện có 3 sân bay, Quy định max 3.
                // Expect: Có thông báo Warning, số lượng giữ nguyên.
                yield return new TestCaseData(3, 3, "Số sân bay trung gian tối đa là: 3")
                    .SetName("UTCID02_CountEqualsLimit_ShowWarning");
            }
        }

        // --- TEST METHOD (Gọi dữ liệu từ trên xuống) ---
        [Test]
        [TestCaseSource(nameof(AirportTestCases))]
        public void AddIntermediateAirport_FromExcelData(int initialCount, int maxLimit, string expectedWarningMessage)
        {
            // 1. Arrange: Cấu hình dữ liệu giả dựa trên tham số đầu vào

            // Setup Quy định trong DB giả
            var listQuyDinh = new List<Quydinh> { new Quydinh { Id = 1, SoSanBayTgtoiDa = maxLimit } };
            _mockContext.Setup(c => c.Quydinhs).Returns(CreateMockDbSet(listQuyDinh).Object);

            // Setup danh sách hiện tại trong ViewModel
            _viewModel.DanhSachSBTG = new ObservableCollection<SBTG>();
            for (int i = 0; i < initialCount; i++)
            {
                _viewModel.DanhSachSBTG.Add(new SBTG { STT = i + 1 });
            }

            // 2. Act: Gọi hàm cần test
            _viewModel.AddIntermediateAirport();

            // 3. Assert: Kiểm tra kết quả
            if (expectedWarningMessage == null)
            {
                // --- TRƯỜNG HỢP MONG ĐỢI THÀNH CÔNG (Happy Path) ---

                // Kiểm tra số lượng tăng lên 1 đơn vị
                Assert.AreEqual(initialCount + 1, _viewModel.DanhSachSBTG.Count,
                    "Số lượng SBTG phải tăng lên khi chưa đạt giới hạn.");

                // Đảm bảo KHÔNG gọi cảnh báo Warning
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, It.IsAny<bool>()),
                    Times.Never,
                    "Không được hiện cảnh báo khi thêm hợp lệ.");
            }
            else
            {
                // --- TRƯỜNG HỢP MONG ĐỢI CÓ CẢNH BÁO (Validation Failed) ---

                // Kiểm tra số lượng KHÔNG đổi
                Assert.AreEqual(initialCount, _viewModel.DanhSachSBTG.Count,
                    "Số lượng không được tăng khi đã chạm giới hạn.");

                // Kiểm tra Notification được gọi đúng thông điệp
                _mockNotificationService.Verify(
                    n => n.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains(expectedWarningMessage)),
                        NotificationType.Warning,
                        It.IsAny<bool>()
                    ),
                    Times.Once,
                    $"Phải hiển thị cảnh báo chứa: '{expectedWarningMessage}'"
                );
            }
        }

        // --- HELPER: Mock DbSet (Cần thiết cho EF Core Mocking) ---
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }
    }
}
