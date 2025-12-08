using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Navigation;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Booking;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class PassengerInformationViewModelTests
    {
        private Mock<INotificationService> _mockNotificationService;
        private Mock<IAirTicketDbContextService> _mockDbContextService;
        private PassengerInformationViewModel _viewModel;
        private ThongTinChuyenBayDuocChon _flightInfoStub;

        [SetUp]
        public void Setup()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockDbContextService = new Mock<IAirTicketDbContextService>();

            // Mock Notification luôn trả về true
            _mockNotificationService
                .Setup(x => x.ShowNotificationAsync(It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            // Giả lập chuyến bay
            _flightInfoStub = new ThongTinChuyenBayDuocChon
            {
                Flight = new KQTraCuuChuyenBayMoRong
                {
                    NumberAdults = 1,
                    NumberChildren = 1,
                    NumberInfants = 1,
                    NgayDi = DateTime.Today.AddDays(10)
                },
                TicketClass = new HangVe { TenHangVe = "Eco" }
            };

            // Khởi tạo ViewModel
            _viewModel = new PassengerInformationViewModel(
                _flightInfoStub,
                _mockDbContextService.Object,
                _mockNotificationService.Object,
                new NotificationViewModel(),
                () => { }
            );

            // ================================================================
            // THIẾT LẬP THỦ CÔNG CÁC MỐC THỜI GIAN QUY ĐỊNH (QUAN TRỌNG)
            // ================================================================

            var today = DateTime.Today;

            // 1. Quy định Em bé: < 2 tuổi
            _viewModel.NgayBatDauEmBe = today.AddYears(-2);
            _viewModel.NgayKetThucEmBe = today;

            // 2. Quy định Trẻ em: 2 - 12 tuổi
            _viewModel.NgayBatDauTreEm = today.AddYears(-12);
            _viewModel.NgayKetThucTreEm = _viewModel.NgayBatDauEmBe.AddDays(-1);

            // 3. Quy định Người lớn: > 12 tuổi (đến 100 tuổi)
            _viewModel.NgayBatDauNguoiLon = today.AddYears(-100);
            _viewModel.NgayKetThucNguoiLon = _viewModel.NgayBatDauTreEm.AddDays(-1);
        }

        // =========================================================================
        // =========================================================================
        public static IEnumerable<TestCaseData> ExcelTestCases
        {
            get
            {
                // Dữ liệu mẫu HỢP LỆ dùng chung
                string validEmail = "ngocvo2502@gmail.com";
                string validPhone = "0987682438";
                // Người lớn (35 tuổi)
                string validAName = "Vo Xuan Ngoc";
                string validAGender = "Nam";
                string validADoB = "01/01/1990";
                string validAID = "052205003846";
                // Trẻ em (10 tuổi)
                string validCName = "Vo Xuan B";
                string validCGender = "Nam";
                string validCDoB = "27/10/2015";
                // Em bé (Gần 1 tuổi)
                string validIName = "Vo Xuan A";
                string validIGender = "Nam";
                string validIDoB = "27/02/2025";
                string validIGuardian = "Vo Xuan Ngoc";

                // --- GROUP 1: CONTACT EMAIL ---
                yield return CreateCase("UTCID01", null, validPhone, "Vui lòng nhập đầy đủ thông tin");
                yield return CreateCase("UTCID02", "", validPhone, "Vui lòng nhập đầy đủ thông tin");
                yield return CreateCase("UTCID03", "ngocvo2502@", validPhone, "Email không hợp lệ!");

                // --- GROUP 2: CONTACT PHONE ---
                yield return CreateCase("UTCID04", validEmail, null, "Vui lòng nhập đầy đủ thông tin");
                yield return CreateCase("UTCID05", validEmail, "", "Vui lòng nhập đầy đủ thông tin");
                yield return CreateCase("UTCID06", validEmail, "098768243a", "Số điện thoại không hợp lệ!");

                // --- GROUP 3: ADULT INFO ---
                yield return CreateCase("UTCID07", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aName: null);
                yield return CreateCase("UTCID08", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aName: "");
                yield return CreateCase("UTCID09", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aGender: null);
                yield return CreateCase("UTCID10", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aDoB: null);
                yield return CreateCase("UTCID11", validEmail, validPhone, "Ngày sinh của hành khách", aDoB: "02/11/2026"); // Tương lai
                yield return CreateCase("UTCID12", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aID: null);
                yield return CreateCase("UTCID13", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", aID: "");
                yield return CreateCase("UTCID14", validEmail, validPhone, "Số căn cước không hợp lệ!", aID: "05220500384"); // Thiếu số
                yield return CreateCase("UTCID15", validEmail, validPhone, "Số căn cước không hợp lệ!", aID: "05220500384a"); // Có chữ

                // --- GROUP 4: CHILD INFO ---
                yield return CreateCase("UTCID16", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", cName: null);
                yield return CreateCase("UTCID17", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", cName: "");
                yield return CreateCase("UTCID18", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", cGender: null);
                yield return CreateCase("UTCID19", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", cDoB: null);
                yield return CreateCase("UTCID20", validEmail, validPhone, "Ngày sinh của hành khách", cDoB: "02/11/2026"); // Tương lai

                // --- GROUP 5: INFANT INFO ---
                yield return CreateCase("UTCID21", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", iName: null);
                yield return CreateCase("UTCID22", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", iName: "");
                yield return CreateCase("UTCID23", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", iGender: null);
                yield return CreateCase("UTCID24", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", iDoB: null);
                yield return CreateCase("UTCID25", validEmail, validPhone, "Ngày sinh của hành khách", iDoB: "02/11/2026"); // Tương lai
                yield return CreateCase("UTCID26", validEmail, validPhone, "Vui lòng nhập đầy đủ thông tin", iGuardian: null);

                // --- GROUP 6: HAPPY PATHS (Thành công - Mong đợi msg lỗi là NULL) ---
                yield return CreateCase("UTCID27", validEmail, validPhone, null, aGender: "Nam", cGender: "Nam", iGender: "Nam");
                yield return CreateCase("UTCID28", validEmail, validPhone, null, aGender: "Nam", cGender: "Nữ", iGender: "Nữ");
                yield return CreateCase("UTCID29", validEmail, validPhone, null, aGender: "Nữ", cGender: "Nữ", iGender: "Nam");
                yield return CreateCase("UTCID30", validEmail, validPhone, null, aGender: "Nữ", cGender: "Nam", iGender: "Nữ");
            }
        }

        // =========================================================================
        // 2. TEST METHOD: CHẠY TEST TỪ NGUỒN DỮ LIỆU TRÊN
        // =========================================================================
        [Test]
        [TestCaseSource(nameof(ExcelTestCases))]
        public async Task ValidatePassengerInfo_FromExcelData(
            string email, string phone,
            string aName, string aGender, string aDoB, string aID,
            string cName, string cGender, string cDoB,
            string iName, string iGender, string iDoB, string iGuardianName,
            string expectedErrorMessage)
        {
            // 1. Arrange: Map dữ liệu vào ViewModel
            _viewModel.ContactEmail = email;
            _viewModel.ContactPhone = phone;

            // Adult
            var adult = _viewModel.PassengerList.First(p => p.IsAdult);
            adult.FullName = aName;
            adult.Gender = aGender;
            adult.DateOfBirth = ParseDate(aDoB);
            adult.IdentityNumber = aID;

            // Child
            var child = _viewModel.PassengerList.First(p => p.IsChild);
            child.FullName = cName;
            child.Gender = cGender;
            child.DateOfBirth = ParseDate(cDoB);

            // Infant
            var infant = _viewModel.PassengerList.First(p => p.IsInfant);
            infant.FullName = iName;
            infant.Gender = iGender;
            infant.DateOfBirth = ParseDate(iDoB);

            // Guardian
            if (!string.IsNullOrEmpty(iGuardianName))
            {
                var guardian = _viewModel.PassengerList.FirstOrDefault(p => p.IsAdult && p.FullName == iGuardianName);
                infant.AccompanyingAdult = guardian;
            }
            else
            {
                infant.AccompanyingAdult = null;
            }

            // 2. Act: Gọi hàm Validate
            try
            {
                await _viewModel.ValidatePassengerInfoAndProceedCommand.ExecuteAsync(null);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi Navigation nếu validation pass (vì NavigationService chưa được mock/setup đầy đủ trong ngữ cảnh tĩnh)
            }

            // 3. Assert: Kiểm tra Notification
            if (expectedErrorMessage != null)
            {
                // Mong đợi LỖI
                _mockNotificationService.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains(expectedErrorMessage)),
                        NotificationType.Warning,
                        false),
                    Times.Once,
                    $"Lỗi mong đợi: '{expectedErrorMessage}' nhưng không thấy xuất hiện.");
            }
            else
            {
                // Mong đợi THÀNH CÔNG (Happy Path) -> Không được hiện lỗi
                _mockNotificationService.Verify(
                    x => x.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, It.IsAny<bool>()),
                    Times.Never,
                    "Mong đợi thành công nhưng lại xuất hiện thông báo lỗi.");
            }
        }

        // =========================================================================
        // 3. HELPER METHODS
        // =========================================================================

        // Helper tạo Test Case nhanh
        private static TestCaseData CreateCase(
            string caseName, string email, string phone, string expectedError,
            string aName = "Vo Xuan Ngoc", string aGender = "Nam", string aDoB = "01/01/1990", string aID = "052205003846",
            string cName = "Vo Xuan B", string cGender = "Nam", string cDoB = "27/10/2015",
            string iName = "Vo Xuan A", string iGender = "Nam", string iDoB = "27/02/2025", string iGuardian = "Vo Xuan Ngoc")
        {
            return new TestCaseData(
                email, phone,
                aName, aGender, aDoB, aID,
                cName, cGender, cDoB,
                iName, iGender, iDoB, iGuardian,
                expectedError
            ).SetName(caseName);
        }

        // Helper chuyển String sang DateTime?
        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;

            // Thử các định dạng ngày tháng phổ biến trong file Excel
            string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

            if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }
    }
}

