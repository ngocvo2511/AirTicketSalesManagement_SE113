using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.Customer;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Admin;
using Moq;
using System.Globalization;
using System.Reflection;

namespace AirTicketSalesManagementTests
{
    [TestFixture]
    public class EditCustomer_test
    {
        private Mock<ICustomerService> _mockCustomerService;
        private Mock<INotificationService> _mockNotificationService;
        private CustomerManagementViewModel _viewModel;
        private Khachhang _selectedCustomerStub;

        [SetUp]
        public void Setup()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockNotificationService = new Mock<INotificationService>();

            _mockCustomerService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Khachhang>());

            _selectedCustomerStub = new Khachhang
            {
                MaKh = 1, // Dùng số nguyên
                HoTenKh = "Old Name",
                Cccd = "000000000000",
                SoDt = "0000000000",
                GioiTinh = "Nam",
                NgaySinh = new DateOnly(1990, 1, 1)
            };

            _mockCustomerService
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(_selectedCustomerStub);

            _viewModel = new CustomerManagementViewModel(
                _mockCustomerService.Object,
                _mockNotificationService.Object
            );


            _viewModel.SelectedCustomer = _selectedCustomerStub;
            _viewModel.IsEditPopupOpen = true;
        }



        public static IEnumerable<TestCaseData> ExcelTestCases
        {
            get
            {
                // Dữ liệu hợp lệ dùng chung
                string validName = "Ngoc Vo";
                string validPhone = "0987682438";
                string validCccd = "052205003846";
                string validDoB = "02/11/2005";
                string validGender = "Nam";


                yield return CreateCase("UTCID01", null, validPhone, validCccd, validDoB, validGender, "Tên không được để trống");
                yield return CreateCase("UTCID02", "", validPhone, validCccd, validDoB, validGender, "Tên không được để trống");
                yield return CreateCase("UTCID03", validName, "0987682438a", validCccd, validDoB, validGender, "Số điện thoại không hợp lệ!");
                yield return CreateCase("UTCID04", validName, validPhone, "05220500384", validDoB, validGender, "Số căn cước công dân không hợp lệ!");
                yield return CreateCase("UTCID05", validName, validPhone, "05220500384a", validDoB, validGender, "Số căn cước công dân không hợp lệ!");
                yield return CreateCase("UTCID06", validName, validPhone, validCccd, "26/12/2026", validGender, "Ngày sinh không hợp lệ!");

                // === GROUP 2: HAPPY PATHS (Mong đợi Thành công -> Có lưu DB) ===

                // UTCID07, 08, 09: Các bộ dữ liệu hợp lệ
                yield return CreateCase("UTCID07", validName, null, null, null, "Nữ", null);
                yield return CreateCase("UTCID08", validName, "", "", validDoB, "Nam", null);
                yield return CreateCase("UTCID09", validName, validPhone, validCccd, validDoB, "Khác", null);
            }
        }

        [Test]
        [TestCaseSource(nameof(ExcelTestCases))]
        public async Task EditCustomer_FromExcelData(
            string name, string phone, string cccd, string dob, string gender,
            string expectedErrorMessage)
        {
            // 1. Arrange: Đổ dữ liệu vào ViewModel
            _viewModel.EditName = name;
            _viewModel.EditPhone = phone;
            _viewModel.EditCccd = cccd;
            _viewModel.EditGender = gender;
            _viewModel.EditBirthDate = ParseDate(dob);


            await _viewModel.SaveEditCustomer();


            // 3. Assert: Kiểm tra logic
            if (expectedErrorMessage != null)
            {
                // --- TRƯỜNG HỢP MONG ĐỢI LỖI (Validation Failed) ---
                // Logic đúng là: Validation chặn lại -> KHÔNG gọi hàm UpdateAsync xuống DB.

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never,
                    $"Lỗi mong đợi: '{expectedErrorMessage}', nhưng hàm UpdateAsync vẫn được gọi (Validation bị bỏ qua).");
            }
            else
            {
                // --- TRƯỜNG HỢP MONG ĐỢI THÀNH CÔNG (Happy Path) ---
                // Logic đúng là: Validation qua -> Gọi hàm UpdateAsync xuống DB -> Sau đó mới hiện thông báo thành công.

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.Is<Khachhang>(k => k.HoTenKh == name)),
                    Times.Once,
                    "Hàm UpdateAsync chưa được gọi (Lưu thất bại).");

                _mockNotificationService.Verify(
           n => n.ShowNotificationAsync(
               It.Is<string>(msg => msg.Contains("Cập nhật khách hàng thành công!")),
               NotificationType.Information,
               It.IsAny<bool>()),
           Times.Once,
           "Phải hiển thị thông báo thành công."
       );
            }
        }

        // --- HELPERS ---
        private static TestCaseData CreateCase(string caseName, string name, string phone, string cccd, string dob, string gender, string expectedError)
        {
            return new TestCaseData(name, phone, cccd, dob, gender, expectedError).SetName(caseName);
        }

        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };
            if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            return null;
        }
    }
}
