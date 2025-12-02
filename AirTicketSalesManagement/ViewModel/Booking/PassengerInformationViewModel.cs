using AirTicketSalesManagement.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AirTicketSalesManagement.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using AirTicketSalesManagement.Data;
using System.Text.RegularExpressions;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;

namespace AirTicketSalesManagement.ViewModel.Booking
{
    public partial class PassengerInformationViewModel : BaseViewModel
    {
        // --- Dependencies injected for testability ---
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;
        private readonly Action _navigateBackAction;

        public string FlightCode { get; set; }

        [ObservableProperty]
        private ObservableCollection<PassengerInfoModel> passengerList;

        [ObservableProperty]
        private string contactEmail;

        [ObservableProperty]
        private string contactPhone;

        public HangVe SelectedTicketClass { get; set; }

        [ObservableProperty]
        public string diemDi;

        [ObservableProperty]
        public string diemDen;
        [ObservableProperty]
        public DateTime thoiGian;
        [ObservableProperty]
        public string flightSummary;
        [ObservableProperty]
        public DateTime ngayBatDauNguoiLon;
        [ObservableProperty]
        public DateTime ngayKetThucNguoiLon;
        [ObservableProperty]
        public DateTime ngayBatDauTreEm;
        [ObservableProperty]
        public DateTime ngayKetThucTreEm;
        [ObservableProperty]
        public DateTime ngayBatDauEmBe;
        [ObservableProperty]
        public DateTime ngayKetThucEmBe;

        public ThongTinChuyenBayDuocChon ThongTinChuyenBayDuocChon { get; set; }

        // UI notification view model (kept for binding). Use _notificationService to show messages (injectable).
        public NotificationViewModel Notification { get;}

        // Default ctor - safe for designer / tests (uses null/default services)
        public PassengerInformationViewModel()
            : this(null, new AirTicketDbService(),
                   null,
                   new NotificationViewModel(),
                   null)
        {
        }


        // Backwards-compatible ctor used in the app (keeps original behavior)
        public PassengerInformationViewModel(ThongTinChuyenBayDuocChon flight)
            : this(
                flight,
                new AirTicketDbService(),
                null,
                new NotificationViewModel(),
                null)
        {
        }



        // New DI-friendly ctor for unit tests and DI containers
        public PassengerInformationViewModel(
            ThongTinChuyenBayDuocChon? selectedFlightInfo,
            IAirTicketDbContextService? dbContextService,
            INotificationService? notificationService,
            NotificationViewModel notificationVm,
            Action? navigateBackAction)
        {
            // Provide test-friendly defaults (do not throw)
            _dbContextService = dbContextService ?? new AirTicketDbService();
            _notificationService = notificationService ?? new NotificationService(notificationVm);
            _navigateBackAction = navigateBackAction ?? (() => NavigationService.NavigateBack());
            Notification = notificationVm;
            if (selectedFlightInfo == null)
                return;

            ThongTinChuyenBayDuocChon = selectedFlightInfo;
            // Lưu thông tin chuyến bay và hạng vé
            FlightCode = $"{selectedFlightInfo.Flight.MaSBDi} - {selectedFlightInfo.Flight.MaSBDen} ({selectedFlightInfo.Flight.HangHangKhong})";
            SelectedTicketClass = selectedFlightInfo.TicketClass;
            DiemDi = selectedFlightInfo.Flight.DiemDi;
            DiemDen = selectedFlightInfo.Flight.DiemDen;
            thoiGian = selectedFlightInfo.Flight.NgayDi;
            FlightSummary = $"{DiemDi} đến {DiemDen} - {ThoiGian.ToString("dddd, dd 'tháng' MM, yyyy", new CultureInfo("vi-VN"))}";
            // Khởi tạo danh sách hành khách dựa trên số lượng người lớn, trẻ em, em bé
            InitializePassengerList(selectedFlightInfo.Flight.NumberAdults, selectedFlightInfo.Flight.NumberChildren, selectedFlightInfo.Flight.NumberInfants); // Thay bằng dữ liệu thực tế nếu cần

            // Load regulation via injected service (test can provide fake context)
            try
            {
                using var context = _dbContextService.CreateDbContext();
                var quyDinh = context.Quydinhs.FirstOrDefault();
                if (quyDinh != null)
                {
                    var today = DateTime.Today;

                    NgayBatDauEmBe = today.AddYears(-quyDinh.TuoiToiDaSoSinh.GetValueOrDefault(2)); // sơ sinh
                    NgayKetThucTreEm = NgayBatDauEmBe.AddDays(-1); // trẻ em bắt đầu sau em bé

                    NgayBatDauTreEm = today.AddYears(-quyDinh.TuoiToiDaTreEm.GetValueOrDefault(12)); // trẻ em
                    NgayKetThucNguoiLon = NgayBatDauTreEm.AddDays(-1); // người lớn bắt đầu sau trẻ em

                    NgayBatDauNguoiLon = today.AddYears(-100); // giới hạn 100 tuổi
                    NgayKetThucEmBe = today;

                    // Set provider for model to avoid DB inside model
                    RegulationProvider.TuoiToiDaSoSinh = quyDinh.TuoiToiDaSoSinh.GetValueOrDefault(2);
                    RegulationProvider.TuoiToiDaTreEm = quyDinh.TuoiToiDaTreEm.GetValueOrDefault(12);
                }
            }
            catch
            {
                // swallow to keep ctor test-friendly; notification can be shown by caller if desired
            }
        }

        public PassengerInformationViewModel(ThongTinHanhKhachVaChuyenBay thongTinHanhKhachVaChuyenBay)
            : this(thongTinHanhKhachVaChuyenBay.FlightInfo,new AirTicketDbService(), null, new NotificationViewModel(), null)
        {
            AddExistingInformation(thongTinHanhKhachVaChuyenBay);
        }

        private void AddExistingInformation(ThongTinHanhKhachVaChuyenBay thongTinHanhKhachVaChuyenBay)
        {
            if (thongTinHanhKhachVaChuyenBay?.PassengerList == null || PassengerList == null)
                return;

            for (int i = 0; i < thongTinHanhKhachVaChuyenBay.PassengerList.Count && i < PassengerList.Count; i++)
            {
                var source = thongTinHanhKhachVaChuyenBay.PassengerList[i];
                var target = PassengerList[i];

                target.FullName = source.HoTen;
                target.Gender = source.GioiTinh;
                target.DateOfBirth = source.NgaySinh;
                target.IdentityNumber = source.CCCD;

                // Nếu là Infant thì kiểm tra thông tin người đi kèm
                if (target.PassengerType == PassengerType.Infant && !string.IsNullOrEmpty(source.HoTenNguoiGiamHo))
                {
                    var matchingAdult = PassengerList
                        .FirstOrDefault(p => p.PassengerType == PassengerType.Adult && p.FullName == source.HoTenNguoiGiamHo);

                    target.AccompanyingAdult = matchingAdult;
                }
            }

            ContactEmail = thongTinHanhKhachVaChuyenBay.ContactEmail;
            ContactPhone = thongTinHanhKhachVaChuyenBay.ContactPhone;
        }

        private void InitializePassengerList(int adultCount, int childCount, int infantCount)
        {
            PassengerList = new ObservableCollection<PassengerInfoModel>();

            // Add adults
            for (int i = 0; i < adultCount; i++)
            {
                PassengerList.Add(new PassengerInfoModel
                {
                    PassengerType = PassengerType.Adult,
                    Index = i + 1,
                    GenderOptions = new List<string> { "Nam", "Nữ" }
                });
            }

            // Add children
            for (int i = 0; i < childCount; i++)
            {
                PassengerList.Add(new PassengerInfoModel
                {
                    PassengerType = PassengerType.Child,
                    Index = i + 1,
                    GenderOptions = new List<string> { "Nam", "Nữ" }
                });
            }

            // Add infants
            for (int i = 0; i < infantCount; i++)
            {
                var infant = new PassengerInfoModel
                {
                    PassengerType = PassengerType.Infant,
                    Index = i + 1,
                    GenderOptions = new List<string> { "Nam", "Nữ" }
                };

                // Set list of adults for accompanying dropdown
                infant.AdultPassengers = PassengerList.Where(p => p.PassengerType == PassengerType.Adult).ToList();

                PassengerList.Add(infant);
            }
        }

        [RelayCommand]
        private void Back()
        {
            // Use injected action (testable) or fallback to static NavigationService
            _navigateBackAction();
        }

        [RelayCommand]
        public async Task ValidatePassengerInfoAndProceed()
        {
            // Validate all required fields are filled
            if (string.IsNullOrWhiteSpace(ContactEmail) || string.IsNullOrWhiteSpace(ContactPhone))
            {
                await _notificationService.ShowNotificationAsync("Vui lòng nhập đầy đủ thông tin", NotificationType.Warning);
                return;
            }

            if (!IsValidEmail(ContactEmail))
            {
                await _notificationService.ShowNotificationAsync("Email không hợp lệ!", NotificationType.Warning);
                return;
            }

            if (!IsValidPhone(ContactPhone))
            {
                await _notificationService.ShowNotificationAsync("Số điện thoại không hợp lệ!", NotificationType.Warning);
                return;
            }

            List<HanhKhach> passengerList = new List<HanhKhach>();

            foreach (var passenger in PassengerList)
            {
                if (string.IsNullOrWhiteSpace(passenger.FullName) || passenger.Gender == null ||
                    passenger.DateOfBirth == null)
                {
                    await _notificationService.ShowNotificationAsync("Vui lòng nhập đầy đủ thông tin", NotificationType.Warning);
                    return;
                }

                if (!IsValidPassengerDateOfBirth(passenger))
                {
                    await _notificationService.ShowNotificationAsync($"Ngày sinh của hành khách {passenger.FullName} không hợp lệ với độ tuổi loại {passenger.PassengerTypeText}.", NotificationType.Warning);
                    return;
                }

                // Additional validation for adults
                if (passenger.PassengerType == PassengerType.Adult)
                {
                    if (string.IsNullOrWhiteSpace(passenger.IdentityNumber))
                    {
                        await _notification_service_fallback($"Vui lòng nhập đầy đủ thông tin", NotificationType.Warning);
                        return;
                    }
                    else if (passenger.IdentityNumber.Length != 12 || !passenger.IdentityNumber.All(char.IsDigit))
                    {
                        await _notificationService.ShowNotificationAsync("Số căn cước không hợp lệ!", NotificationType.Warning);
                        return;
                    }
                }



                // Additional validation for infants
                if (passenger.PassengerType == PassengerType.Infant &&
                    passenger.AccompanyingAdult == null)
                {
                    await _notificationService.ShowNotificationAsync("Vui lòng nhập đầy đủ thông tin", NotificationType.Warning);
                    return;
                }
                passengerList.Add(new HanhKhach(passenger.FullName, passenger.DateOfBirth.Value, passenger.Gender, passenger.IdentityNumber, passenger.AccompanyingAdult?.FullName));
            }

            ThongTinHanhKhachVaChuyenBay thongTinHanhKhachVaChuyenBay = new ThongTinHanhKhachVaChuyenBay
            {
                FlightInfo = ThongTinChuyenBayDuocChon,
                PassengerList = passengerList,
                ContactEmail = ContactEmail,
                ContactPhone = ContactPhone
            };

            NavigationService.NavigateTo<PaymentConfirmationViewModel>(thongTinHanhKhachVaChuyenBay);
        }

        // Compatibility helper: some callers may have bound NotificationViewModel to UI but not INotificationService.
        // This helper will use _notificationService and fall back to Notification.ShowNotificationAsync if needed.
        private async Task<bool> _notification_service_fallback(string message, NotificationType type, bool isConfirmation = false)
        {
            try
            {
                return await _notificationService.ShowNotificationAsync(message, type, isConfirmation);
            }
            catch
            {
                // fallback to UI viewmodel if service implementation throws or not available
                return await Notification.ShowNotificationAsync(message, type, isConfirmation);
            }
        }

        public bool IsValidPassengerDateOfBirth(PassengerInfoModel passenger)
        {
            if (passenger.DateOfBirth == null)
                return false;

            var dob = passenger.DateOfBirth.Value.Date;

            return passenger.PassengerType switch
            {
                PassengerType.Adult => dob >= NgayBatDauNguoiLon && dob <= NgayKetThucNguoiLon,
                PassengerType.Child => dob >= NgayBatDauTreEm && dob <= NgayKetThucTreEm,
                PassengerType.Infant => dob >= NgayBatDauEmBe && dob <= NgayKetThucEmBe,
                _ => false
            };
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Chuẩn hóa
            email = email.Trim();

            // Regex chặt chẽ hơn MailAddress
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$");
        }

        // Small in-memory provider to avoid DB access in model property.
        // Tests can rely on ctor to set these values via _dbContextService.
        internal static class RegulationProvider
        {
            public static int TuoiToiDaSoSinh { get; set; } = 2;
            public static int TuoiToiDaTreEm { get; set; } = 12;
        }
    }

    public partial class PassengerInfoModel : ObservableObject
    {
        public int Index { get; set; }
        public PassengerType PassengerType { get; set; }

        public bool IsAdult => PassengerType == PassengerType.Adult;
        public bool IsChild => PassengerType == PassengerType.Child;
        public bool IsInfant => PassengerType == PassengerType.Infant;

        public string PassengerTypeText
        {
            get
            {
                return PassengerType switch
                {
                    PassengerType.Adult => "Người lớn",
                    PassengerType.Child => "Trẻ em",
                    PassengerType.Infant => "Em bé",
                    _ => string.Empty
                };
            }
        }

        // Use RegulationProvider instead of creating DbContext inside model
        public string PassengerTypeDescription
        {
            get
            {
                int tuoiToiDaSoSinh = PassengerInformationViewModel.RegulationProvider.TuoiToiDaSoSinh;
                int tuoiToiDaTreEm = PassengerInformationViewModel.RegulationProvider.TuoiToiDaTreEm;

                return PassengerType switch
                {
                    PassengerType.Adult => $"Từ {tuoiToiDaTreEm} tuổi trở lên",
                    PassengerType.Child => $"Từ {tuoiToiDaSoSinh}-{tuoiToiDaTreEm} tuổi",
                    PassengerType.Infant => $"Dưới {tuoiToiDaSoSinh} tuổi",
                    _ => string.Empty
                };
            }
        }

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string gender;

        public List<string> GenderOptions { get; set; }

        [ObservableProperty]
        private DateTime? dateOfBirth;

        [ObservableProperty]
        private string identityNumber;


        // For infants only
        public List<PassengerInfoModel> AdultPassengers { get; set; }

        [ObservableProperty]
        private PassengerInfoModel accompanyingAdult;
    }

    public enum PassengerType
    {
        Adult,
        Child,
        Infant
    }
}
