using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.Customer;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel;
using AirTicketSalesManagement.ViewModel.Admin;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            // 1. Setup Mock Services
            _mockCustomerService = new Mock<ICustomerService>();
            _mockNotificationService = new Mock<INotificationService>();

            // Mock GetAll trả về danh sách rỗng
            _mockCustomerService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Khachhang>());

            // --- GIẢ LẬP DỮ LIỆU KHÁCH HÀNG (Dùng ID là int theo yêu cầu) ---
            _selectedCustomerStub = new Khachhang
            {
                MaKh = 1, // Dùng số nguyên
                HoTenKh = "Old Name",
                Cccd = "000000000000",
                SoDt = "0000000000",
                GioiTinh = "Nam",
                NgaySinh = new DateOnly(1990, 1, 1)
            };

            // Setup GetById: Chấp nhận bất kỳ int nào và trả về khách hàng trên
            _mockCustomerService
                .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(_selectedCustomerStub);

            // 2. Khởi tạo ViewModel
            // Lưu ý: ViewModel sẽ tạo ra NotificationViewModel "thật" bên trong constructor
            _viewModel = new CustomerManagementViewModel(
                _mockCustomerService.Object,
                _mockNotificationService.Object
            );

            // 3. --- CHIẾN THUẬT NULL OBJECT ---
            // Thay vì Mock (gây lỗi NotSupportedException), ta gán NULL vào Notification.
            // Khi code chạy đến dòng await Notification.ShowNotificationAsync(...), nó sẽ văng NullReferenceException.
            // Ta sẽ catch lỗi này trong Test để tránh treo chương trình.

            bool replaced = ReplaceBackingField(_viewModel, "Notification", null);

            if (!replaced)
            {
                // Nếu không thay thế được, Fail test ngay để kiểm tra
                Assert.Fail("KHÔNG THỂ gán null cho Notification bằng Reflection.");
            }

            // Gán ngữ cảnh (Khách hàng đang được chọn để sửa)
            _viewModel.SelectedCustomer = _selectedCustomerStub;
            _viewModel.IsEditPopupOpen = true;
        }

        // Hàm helper tìm và thay thế biến ẩn (Backing Field) bên trong ViewModel
        private bool ReplaceBackingField(object target, string propertyName, object newValue)
        {
            var type = target.GetType();

            // 1. Cố gắng tìm field theo quy tắc <Name>k__BackingField (Compiler generate)
            var fieldName = $"<{propertyName}>k__BackingField";
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (field != null)
            {
                field.SetValue(target, newValue);
                return true;
            }

            // 2. Fallback: Duyệt qua tất cả field để tìm field có kiểu NotificationViewModel
            var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var f in allFields)
            {
                if (f.FieldType == typeof(NotificationViewModel))
                {
                    f.SetValue(target, newValue);
                    return true;
                }
            }
            return false;
        }

        // --- TEST DATA (Lấy từ file Excel Lab 3) ---
        public static IEnumerable<TestCaseData> ExcelTestCases
        {
            get
            {
                // Dữ liệu hợp lệ dùng chung
                string validName = "Ngoc Vo";
                string validPhone = "0987682438";
                string validCccd = "052205003846";
                string validDoB = "11/02/2005";
                string validGender = "Nam";

                // === GROUP 1: VALIDATION CASES (Mong đợi Lỗi -> Không lưu DB) ===

                // UTCID01: Tên null
                yield return CreateCase("UTCID01", null, validPhone, validCccd, validDoB, validGender, "Tên không được để trống");
                // UTCID02: Tên rỗng
                yield return CreateCase("UTCID02", "", validPhone, validCccd, validDoB, validGender, "Tên không được để trống");
                // UTCID03: SĐT chứa chữ
                yield return CreateCase("UTCID03", validName, "0987682438a", validCccd, validDoB, validGender, "Số điện thoại không hợp lệ!");
                // UTCID04: CCCD thiếu số
                yield return CreateCase("UTCID04", validName, validPhone, "05220500384", validDoB, validGender, "Số căn cước công dân không hợp lệ!");
                // UTCID05: CCCD chứa chữ
                yield return CreateCase("UTCID05", validName, validPhone, "05220500384a", validDoB, validGender, "Số căn cước công dân không hợp lệ!");
                // UTCID06: Ngày sinh tương lai
                yield return CreateCase("UTCID06", validName, validPhone, validCccd, "26/12/2026", validGender, "Ngày sinh không hợp lệ!");

                // === GROUP 2: HAPPY PATHS (Mong đợi Thành công -> Có lưu DB) ===

                // UTCID07, 08, 09: Các bộ dữ liệu hợp lệ
                yield return CreateCase("UTCID07", "Ngoc Vo", "0987682438", "052205003846", "11/02/2005", "Nữ", null);
                yield return CreateCase("UTCID08", "Nguyen Van A", "0909090909", "012345678912", "01/01/1990", "Nam", null);
                yield return CreateCase("UTCID09", "Tran Thi B", "0912345678", "098765432109", "05/05/1995", "Khác", null);
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

            // 2. Act: Gọi hàm SaveEditCustomer
            try
            {
                await _viewModel.SaveEditCustomer();
            }
            catch (NullReferenceException)
            {
                // BẮT BUỘC CÓ: Do ta gán Notification = null, nên khi code chạy đến dòng hiển thị thông báo, 
                // nó sẽ văng lỗi này. Ta "nuốt" lỗi để test tiếp tục verify.
            }
            catch (Exception ex)
            {
                // In ra lỗi khác (nếu có) để debug
                Console.WriteLine("Test Exception khác: " + ex.Message);
            }

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
