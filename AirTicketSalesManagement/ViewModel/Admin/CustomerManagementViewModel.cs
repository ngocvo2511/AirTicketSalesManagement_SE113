using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Services.Customer;
using AirTicketSalesManagement.Services.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AirTicketSalesManagement.ViewModel.Admin
{
    public partial class CustomerManagementViewModel : BaseViewModel
    {
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notification;
        private ObservableCollection<Khachhang> _customers = new();
        [ObservableProperty]
        private string searchName;
        [ObservableProperty]
        private string searchCccd;
        [ObservableProperty]
        private ObservableCollection<Khachhang> customers = new ObservableCollection<Khachhang>();
        [ObservableProperty]
        private Khachhang selectedCustomer;

        //Edit
        [ObservableProperty]
        private bool isEditPopupOpen = false;
        [ObservableProperty]
        private string? editName;
        [ObservableProperty]
        private string? editCccd;
        [ObservableProperty]
        private string? editPhone;
        [ObservableProperty]
        private string? editGender;
        [ObservableProperty]
        private DateTime? editBirthDate;

        public IRelayCommand LoadCommand { get; }
        public NotificationViewModel Notification { get; }

        public CustomerManagementViewModel(ICustomerService customerService,
                                           INotificationService notification)
        {
            _customerService = customerService;
            _notification = notification;

            // Lấy ViewModel chung từ NotificationService (nếu có)
            if (notification is NotificationService ns)
                Notification = ns.ViewModel;
            else
                Notification = new NotificationViewModel(); // fallback
        }

        [RelayCommand]
        public async Task LoadCustomers()
        {
            try
            {
                var result = await _customerService.GetAllAsync();
                _customers = new ObservableCollection<Khachhang>(result);
                Customers = new ObservableCollection<Khachhang>(result);
            }
            catch (Exception)
            {
                // Có thể log, hoặc show thông báo
                await _notification.ShowNotificationAsync("Lỗi tải dữ liệu khách hàng", NotificationType.Error);
            }
        }
        [RelayCommand]
        public void EditCustomer()
        {
            if (SelectedCustomer == null) return;
            LoadEditField();
            IsEditPopupOpen = true;
        }
        private void LoadEditField()
        {
            if (SelectedCustomer == null) return;
            EditName = SelectedCustomer.HoTenKh;
            EditGender = SelectedCustomer.GioiTinh;
            EditCccd = SelectedCustomer.Cccd;
            EditPhone = SelectedCustomer.SoDt;
            EditBirthDate = SelectedCustomer.NgaySinh?.ToDateTime(new TimeOnly(0, 0));
        }
        [RelayCommand]
        public void Refresh()
        {
            _ = LoadCustomers();
        }
        [RelayCommand]
        public void Search()
        {
            var query = _customers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                query = query.Where(c =>
                    c.HoTenKh?.Contains(SearchName, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrWhiteSpace(SearchCccd))
            {
                query = query.Where(c => c.Cccd?.Contains(SearchCccd) == true);
            }
            Customers.Clear();
            foreach (var c in query)
                Customers.Add(c);
        }
        [RelayCommand]
        public void ClearSearch()
        {
            SearchName = string.Empty;
            SearchCccd = string.Empty;
            Customers = new ObservableCollection<Khachhang>(_customers);
        }

        [RelayCommand]
        public void CancelEdit()
        {
            IsEditPopupOpen = false;
        }
        [RelayCommand]
        public async Task SaveEditCustomer()
        {
            if (SelectedCustomer == null)
            {
                await _notification.ShowNotificationAsync(
                    "Chưa chọn khách hàng để cập nhật.",
                    NotificationType.Warning);
                return;
            }
            string? validationError = ValidateEditCustomer();
            if (validationError != null)
            {
                await _notification.ShowNotificationAsync(
                    validationError,
                    NotificationType.Warning);
                return;
            }
            try
            {
                var customer = await _customerService.GetByIdAsync(SelectedCustomer.MaKh);

                if (customer is null)
                {
                    await _notification.ShowNotificationAsync(
                        "Không tìm thấy khách hàng trong cơ sở dữ liệu.",
                        NotificationType.Error);
                    return;
                }

                // Cập nhật dữ liệu
                customer.HoTenKh = EditName;
                customer.GioiTinh = EditGender;
                customer.Cccd = EditCccd;
                customer.SoDt = EditPhone;
                customer.NgaySinh = EditBirthDate.HasValue ? DateOnly.FromDateTime(EditBirthDate.Value) : null;

                await _customerService.UpdateAsync(customer);

                await _notification.ShowNotificationAsync(
                    "Cập nhật khách hàng thành công!",
                    NotificationType.Information);

                await LoadCustomers();

                IsEditPopupOpen = false;
            }
            catch (Exception ex)
            {
                await _notification.ShowNotificationAsync(
                    "Lỗi khi lưu dữ liệu: " + ex.Message,
                    NotificationType.Error);
            }
        }
        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$");
        }
        public string? ValidateEditCustomer()
        {
            if (string.IsNullOrWhiteSpace(EditName))
                return "Tên không được để trống";

            if (!string.IsNullOrWhiteSpace(EditPhone) && !IsValidPhone(EditPhone))
                return "Số điện thoại không hợp lệ!";

            if (!string.IsNullOrWhiteSpace(EditCccd))
            {
                if (EditCccd.Length != 12 || !EditCccd.All(char.IsDigit))
                    return "Số căn cước công dân không hợp lệ!";
            }

            if (EditBirthDate.HasValue)
            {
                if (EditBirthDate.Value.Date >= DateTime.Today)
                    return "Ngày sinh không hợp lệ!";
            }

            return null; // hợp lệ
        }
    }
}