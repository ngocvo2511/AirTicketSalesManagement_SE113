using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.Customer;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel.Admin;
using Moq;
using System.Collections.ObjectModel;
using System.Globalization;

namespace AirTicketSalesManagementTests.ViewModel.Admin
{
    [TestFixture]
    public class CustomerManagementViewModelTests
    {
        [TestFixture]
        public class EditCustomerTests
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



            [Test]
            public async Task UTCID01_NameNull_ShouldFailValidation()
            {
                _viewModel.EditName = null;
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "052205003846";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID02_NameEmpty_ShouldFailValidation()
            {
                _viewModel.EditName = "";
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "052205003846";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID03_InvalidPhone_ShouldFailValidation()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "0987682438a";
                _viewModel.EditCccd = "052205003846";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID04_InvalidCccdLength_ShouldFailValidation()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "05220500384"; // thiếu
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID05_InvalidCccdCharacter_ShouldFailValidation()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "05220500384a";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID06_BirthDateInFuture_ShouldFailValidation()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "052205003846";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2026, 12, 26);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Never);
            }

            [Test]
            public async Task UTCID07_ValidMinimalData_ShouldUpdate()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = null;
                _viewModel.EditCccd = null;
                _viewModel.EditGender = "Nữ";
                _viewModel.EditBirthDate = null;

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.Is<Khachhang>(k => k.HoTenKh == "Ngoc Vo")),
                    Times.Once);
            }

            [Test]
            public async Task UTCID08_ValidWithPhoneAndDob_ShouldUpdate()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "";
                _viewModel.EditCccd = "";
                _viewModel.EditGender = "Nam";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.IsAny<Khachhang>()),
                    Times.Once);
            }

            [Test]
            public async Task UTCID09_AllValidFields_ShouldUpdate()
            {
                _viewModel.EditName = "Ngoc Vo";
                _viewModel.EditPhone = "0987682438";
                _viewModel.EditCccd = "052205003846";
                _viewModel.EditGender = "Khác";
                _viewModel.EditBirthDate = new DateTime(2005, 11, 2);

                await _viewModel.SaveEditCustomer();

                _mockCustomerService.Verify(
                    s => s.UpdateAsync(It.Is<Khachhang>(k => k.HoTenKh == "Ngoc Vo")),
                    Times.Once);
            }


        }

        [TestFixture]
        public class SearchCustomerTests
        {
            private Mock<ICustomerService> _mockCustomerService;
            private Mock<INotificationService> _mockNotificationService;
            private CustomerManagementViewModel _viewModel;
            private List<Khachhang> _customerList;

            [SetUp]
            public void Setup()
            {
                _mockCustomerService = new Mock<ICustomerService>();
                _mockNotificationService = new Mock<INotificationService>();

                _customerList = new List<Khachhang>
                {
                    new Khachhang { MaKh = 1, HoTenKh = "Nguyen Van A", Cccd = "123456789012" },
                    new Khachhang { MaKh = 2, HoTenKh = "Tran Thi B", Cccd = "987654321098" },
                    new Khachhang { MaKh = 3, HoTenKh = "Le Van C", Cccd = "123123123123" }
                };

                _mockCustomerService.Setup(s => s.GetAllAsync()).ReturnsAsync(_customerList);

                _viewModel = new CustomerManagementViewModel(
                    _mockCustomerService.Object,
                    _mockNotificationService.Object
                );

                // Simulate loaded customers
                typeof(CustomerManagementViewModel)
                    .GetField("_customers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(_viewModel, new ObservableCollection<Khachhang>(_customerList));
                _viewModel.Customers = new ObservableCollection<Khachhang>(_customerList);
            }

            [Test]
            public void Search_ByName_FiltersCorrectly()
            {
                _viewModel.SearchName = "Nguyen";
                _viewModel.SearchCccd = string.Empty;

                _viewModel.Search();

                Assert.That(_viewModel.Customers.Count, Is.EqualTo(1));
                Assert.That(_viewModel.Customers[0].HoTenKh, Is.EqualTo("Nguyen Van A"));
            }

            [Test]
            public void Search_ByCccd_FiltersCorrectly()
            {
                _viewModel.SearchName = string.Empty;
                _viewModel.SearchCccd = "987654321098";

                _viewModel.Search();

                Assert.That(_viewModel.Customers.Count, Is.EqualTo(1));
                Assert.That(_viewModel.Customers[0].Cccd, Is.EqualTo("987654321098"));
            }

            [Test]
            public void Search_ByNameAndCccd_FiltersCorrectly()
            {
                _viewModel.SearchName = "Le";
                _viewModel.SearchCccd = "123123123123";

                _viewModel.Search();

                Assert.That(_viewModel.Customers.Count, Is.EqualTo(1));
                Assert.That(_viewModel.Customers[0].HoTenKh, Is.EqualTo("Le Van C"));
            }

            [Test]
            public void Search_EmptyFilters_ReturnsAll()
            {
                _viewModel.SearchName = string.Empty;
                _viewModel.SearchCccd = string.Empty;

                _viewModel.Search();

                Assert.That(_viewModel.Customers.Count, Is.EqualTo(_customerList.Count));
            }

            [Test]
            public void Search_NoMatch_ReturnsEmpty()
            {
                _viewModel.SearchName = "Nonexistent";
                _viewModel.SearchCccd = "000000000000";

                _viewModel.Search();

                Assert.That(_viewModel.Customers.Count, Is.EqualTo(0));
            }
        }
    }
}
