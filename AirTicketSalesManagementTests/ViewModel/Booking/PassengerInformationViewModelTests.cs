using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using AirTicketSalesManagement.ViewModel.Booking;
using AirTicketSalesManagement.ViewModel;
using Moq;
using System.Globalization;

namespace AirTicketSalesManagementTests.ViewModel.Booking
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

        [Test]
        public async Task ValidatePassengerInfo_UTCID01()
        {
            // Vui lòng nhập đầy đủ thông tin (email null)
            await ValidateAndAssert(
                email: null, phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID02()
        {
            // Vui lòng nhập đầy đủ thông tin (email rỗng)
            await ValidateAndAssert(
                email: "", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID03()
        {
            // Vui lòng nhập đầy đủ thông tin (phone null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: null,
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID04()
        {
            // Vui lòng nhập đầy đủ thông tin (phone rỗng)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID05()
        {
            // Email không hợp lệ!
            await ValidateAndAssert(
                email: "ngocvo2502@", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Email không hợp lệ!"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID06()
        {
            // Số điện thoại không hợp lệ
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "098768243a",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Số điện thoại không hợp lệ"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID07()
        {
            // Vui lòng nhập đầy đủ thông tin (aName null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: null, aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID08()
        {
            // Vui lòng nhập đầy đủ thông tin (aName rỗng)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID09()
        {
            // Vui lòng nhập đầy đủ thông tin (aGender null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: null, aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID10()
        {
            // Vui lòng nhập đầy đủ thông tin (aDoB null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: null, aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID11()
        {
            // Ngày sinh của hành khách Vo Xuan Ngoc không hợp lệ với độ tuổi loại Người lớn.
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "02/11/2026", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Ngày sinh của hành khách Vo Xuan Ngoc không hợp lệ với độ tuổi loại Người lớn."
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID12()
        {
            // Vui lòng nhập đầy đủ thông tin (aID null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: null,
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID13()
        {
            // Vui lòng nhập đầy đủ thông tin (aID rỗng)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID14()
        {
            // Số căn cước không hợp lệ! (aID thiếu số)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "05220500384",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Số căn cước không hợp lệ!"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID15()
        {
            // Số căn cước không hợp lệ! (aID có ký tự)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "05220500384a",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Số căn cước không hợp lệ!"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID16()
        {
            // Vui lòng nhập đầy đủ thông tin (cName null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: null, cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID17()
        {
            // Vui lòng nhập đầy đủ thông tin (cName rỗng)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID18()
        {
            // Vui lòng nhập đầy đủ thông tin (cGender null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: null, cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID19()
        {
            // Vui lòng nhập đầy đủ thông tin (cDoB null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: null,
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID20()
        {
            // Ngày sinh của hành khách Vo Xuan B không hợp lệ với độ tuổi loại Trẻ em.
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "02/11/2026",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Ngày sinh của hành khách Vo Xuan B không hợp lệ với độ tuổi loại Trẻ em."
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID21()
        {
            // Vui lòng nhập đầy đủ thông tin (iName null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: null, iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID22()
        {
            // Vui lòng nhập đầy đủ thông tin (iName rỗng)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID23()
        {
            // Vui lòng nhập đầy đủ thông tin (iGender null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: null, iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID24()
        {
            // Vui lòng nhập đầy đủ thông tin (iDoB null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: null, iGuardian: "Vo Xuan Ngoc",
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID25()
        {
            // Ngày sinh của hành khách Vo Xuan A không hợp lệ với độ tuổi loại Em bé.
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "02/11/2026", iGuardian: "Vo Xuan Ngoc",
                expectedError: "Ngày sinh của hành khách Vo Xuan A không hợp lệ với độ tuổi loại Em bé."
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID26()
        {
            // Vui lòng nhập đầy đủ thông tin (iGuardian null)
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: null,
                expectedError: "Vui lòng nhập đầy đủ thông tin"
            );
        }

        [Test]
        public async Task ValidatePassengerInfo_UTCID27()
        {
            // Happy path - không có lỗi
            await ValidateAndAssert(
                email: "ngocvo2502@gmail.com", phone: "0987682438",
                aName: "Vo Xuan Ngoc", aGender: "Nam", aDoB: "01/01/1990", aID: "052205003846",
                cName: "Vo Xuan B", cGender: "Nữ", cDoB: "27/10/2015",
                iName: "Vo Xuan A", iGender: "Nam", iDoB: "27/02/2025", iGuardian: "Vo Xuan Ngoc",
                expectedError: null
            );
        }

        // Helper method dùng chung cho các test trên
        private async Task ValidateAndAssert(
            string email, string phone,
            string aName, string aGender, string aDoB, string aID,
            string cName, string cGender, string cDoB,
            string iName, string iGender, string iDoB, string iGuardian,
            string expectedError)
        {
            _viewModel.ContactEmail = email;
            _viewModel.ContactPhone = phone;

            var adult = _viewModel.PassengerList.First(p => p.IsAdult);
            adult.FullName = aName;
            adult.Gender = aGender;
            adult.DateOfBirth = ParseDate(aDoB);
            adult.IdentityNumber = aID;

            var child = _viewModel.PassengerList.First(p => p.IsChild);
            child.FullName = cName;
            child.Gender = cGender;
            child.DateOfBirth = ParseDate(cDoB);

            var infant = _viewModel.PassengerList.First(p => p.IsInfant);
            infant.FullName = iName;
            infant.Gender = iGender;
            infant.DateOfBirth = ParseDate(iDoB);

            if (!string.IsNullOrEmpty(iGuardian))
            {
                var guardian = _viewModel.PassengerList.FirstOrDefault(p => p.IsAdult && p.FullName == iGuardian);
                infant.AccompanyingAdult = guardian;
            }
            else
            {
                infant.AccompanyingAdult = null;
            }

            try
            {
                await _viewModel.ValidatePassengerInfoAndProceedCommand.ExecuteAsync(null);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi Navigation nếu validation pass
            }

            if (expectedError != null)
            {
                _mockNotificationService.Verify(
                    x => x.ShowNotificationAsync(
                        It.Is<string>(msg => msg.Contains(expectedError)),
                        NotificationType.Warning,
                        false),
                    Times.Once,
                    $"Lỗi mong đợi: '{expectedError}' nhưng không thấy xuất hiện.");
            }
            else
            {
                _mockNotificationService.Verify(
                    x => x.ShowNotificationAsync(It.IsAny<string>(), NotificationType.Warning, It.IsAny<bool>()),
                    Times.Never,
                    "Mong đợi thành công nhưng lại xuất hiện thông báo lỗi.");
            }
        }

        // Helper chuyển String sang DateTime?
        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            string[] formats = { "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };
            if (DateTime.TryParseExact(dateStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }


    }
}
