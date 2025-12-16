using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using AirTicketSalesManagement.Models.UIModels;
using AirTicketSalesManagement.Services;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AirTicketSalesManagement.ViewModel.Admin
{
    public partial class AccountManagementViewModel : BaseViewModel
    {
        // injected dependencies for testability
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string searchEmail;

        [ObservableProperty]
        private string searchRole;

        [ObservableProperty]
        private ObservableCollection<AccountModel> accounts = new();

        [ObservableProperty]
        private AccountModel selectedAccount;

        // Properties for Add Account
        [ObservableProperty]
        private string addEmail;

        [ObservableProperty]
        private string addRole;

        [ObservableProperty]
        private string addPassword;

        [ObservableProperty]
        private string addFullName;

        [ObservableProperty]
        private UserSelectionModel selectedUser;

        [ObservableProperty]
        private bool isAddPopupOpen = false;

        // Properties for Edit Account
        [ObservableProperty]
        private string editEmail;

        [ObservableProperty]
        private string editRole;

        [ObservableProperty]
        private string editPassword;

        [ObservableProperty]
        private string editFullName;

        [ObservableProperty]
        private UserSelectionModel editSelectedUser;

        [ObservableProperty]
        private bool isEditPopupOpen = false;

        // Collections for ComboBoxes
        [ObservableProperty]
        private ObservableCollection<UserSelectionModel> userList = new();

        // Notification VM kept for UI binding (optional). Use _notificationService for showing messages.
        public NotificationViewModel Notification { get; }

        // Parameterless ctor (runtime) -> uses concrete services
        public AccountManagementViewModel()
            : this(new AirTicketDbService(), new NotificationService(new NotificationViewModel()))
        {
        }

        // DI ctor - use in unit tests to inject fakes/mocks
        public AccountManagementViewModel(IAirTicketDbContextService dbContextService, INotificationService notificationService)
        {
            _dbContextService = dbContextService ?? throw new ArgumentNullException(nameof(dbContextService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            // keep Notification property for UI binding if view binds to it
            Notification = (notificationService as NotificationService)?.ViewModel
                       ?? new NotificationViewModel();

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                LoadAccounts();
            }
        }

        private AirTicketDbContext _createContext() => _dbContextService.CreateDbContext();

        public void LoadAccounts()
        {
            using var context = _createContext();
            var danhSach = context.Taikhoans
                .Include(tk => tk.MaNvNavigation)
                .Include(tk => tk.MaKhNavigation)
                .AsEnumerable()
                .Select(tk => AccountModel.FromTaiKhoan(tk))
                .ToList();

            Accounts = new ObservableCollection<AccountModel>(danhSach);
        }

        [RelayCommand]
        private void LoadUserList()
        {
            using var context = _createContext();
            var nhanVienList = context.Nhanviens
                .Select(nv => UserSelectionModel.FromNhanVien(nv))
                .ToList();

            var khachHangList = context.Khachhangs
                .Select(kh => UserSelectionModel.FromKhachHang(kh))
                .ToList();

            UserList = new ObservableCollection<UserSelectionModel>(
                nhanVienList.Concat(khachHangList).OrderBy(u => u.Name));
        }

        [RelayCommand]
        public void Refresh()
        {
            LoadAccounts();
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchEmail = string.Empty;
            SearchRole = string.Empty;
            LoadAccounts();
        }

        [RelayCommand]
        public void Search()
        {
            Accounts.Clear();

            using var context = _createContext();
            var query = context.Taikhoans
                .Include(tk => tk.MaNvNavigation)
                .Include(tk => tk.MaKhNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchEmail))
            {
                query = query.Where(tk => tk.Email.Contains(SearchEmail));
            }

            if (!string.IsNullOrWhiteSpace(SearchRole) && SearchRole != "Tất cả")
            {
                query = query.Where(tk => tk.VaiTro == SearchRole);
            }

            var results = query.ToList();
            foreach (var tk in results)
            {
                Accounts.Add(AccountModel.FromTaiKhoan(tk));
            }
        }

        [RelayCommand]
        public void AddAccount()
        {
            ResetAddFields();
            IsAddPopupOpen = true;
        }

        private void ResetAddFields()
        {
            AddEmail = string.Empty;
            AddRole = string.Empty;
            AddFullName = string.Empty;
            AddPassword = string.Empty;
        }

        [RelayCommand]
        public void CancelAdd()
        {
            IsAddPopupOpen = false;
        }

        [RelayCommand]
        public void CloseAdd()
        {
            IsAddPopupOpen = false;
        }

        [RelayCommand]
        public async void SaveAddAccount()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddEmail) || string.IsNullOrWhiteSpace(AddRole) || string.IsNullOrWhiteSpace(AddPassword) || string.IsNullOrWhiteSpace(AddFullName))
                {
                    await _notification_service_fallback("Vui lòng điền đầy đủ thông tin tài khoản.", NotificationType.Warning);
                    return;
                }

                if (!IsValidEmail(AddEmail))
                {
                    await _notification_service_fallback("Email không hợp lệ!", NotificationType.Warning);
                    return;
                }

                using var context = _createContext();
                // Check if email already exists
                if (context.Taikhoans.Any(tk => tk.Email == AddEmail))
                {
                    await _notification_service_fallback("Email này đã được sử dụng.", NotificationType.Warning);
                    return;
                }

                int? maNv = null;
                int? maKh = null;

                if (AddRole == "Nhân viên" || AddRole == "Admin")
                {
                    var newNhanVien = new Nhanvien
                    {
                        HoTenNv = AddFullName,
                    };

                    context.Nhanviens.Add(newNhanVien);
                    context.SaveChanges();
                    maNv = newNhanVien.MaNv;
                }
                else if (AddRole == "Khách hàng")
                {
                    var newKhachHang = new Khachhang
                    {
                        HoTenKh = AddFullName,
                    };

                    context.Khachhangs.Add(newKhachHang);
                    context.SaveChanges();
                    maKh = newKhachHang.MaKh;
                }

                var newAccount = new Taikhoan
                {
                    Email = AddEmail,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(AddPassword),
                    VaiTro = AddRole,
                    MaNv = maNv,
                    MaKh = maKh
                };

                context.Taikhoans.Add(newAccount);
                context.SaveChanges();

                await _notification_service_fallback("Tài khoản đã được thêm thành công!", NotificationType.Information);
                IsAddPopupOpen = false;
                LoadAccounts();
                LoadUserList();
            }
            catch (Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi thêm tài khoản: " + ex.Message, NotificationType.Error);
            }
        }

        [RelayCommand]
        public async Task EditAccount()
        {
            if (SelectedAccount == null)
            {
                await _notification_service_fallback("Vui lòng chọn một tài khoản để chỉnh sửa.", NotificationType.Warning);
                return;
            }

            ResetEditFields();
            IsEditPopupOpen = true;
        }

        private void ResetEditFields()
        {
            EditEmail = SelectedAccount.Email;
            EditRole = SelectedAccount.VaiTro;
            EditFullName = SelectedAccount.HoTen;

            if (SelectedAccount.MaNv.HasValue)
            {
                EditSelectedUser = UserList.FirstOrDefault(u =>
                    u.Id == SelectedAccount.MaNv.Value && u.Type == "Nhân viên");
            }
            else if (SelectedAccount.MaKh.HasValue)
            {
                EditSelectedUser = UserList.FirstOrDefault(u =>
                    u.Id == SelectedAccount.MaKh.Value && u.Type == "Khách hàng");
            }
        }

        [RelayCommand]
        public void CancelEdit()
        {
            IsEditPopupOpen = false;
        }

        [RelayCommand]
        public void CloseEdit()
        {
            IsEditPopupOpen = false;
        }

        [RelayCommand]
        public async void SaveEditAccount()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditEmail) || string.IsNullOrWhiteSpace(EditRole) || string.IsNullOrWhiteSpace(EditFullName))
                {
                    await _notification_service_fallback("Vui lòng điền đầy đủ thông tin tài khoản.", NotificationType.Warning);
                    return;
                }

                using var context = _createContext();
                var existingAccount = context.Taikhoans
                    .FirstOrDefault(tk => tk.MaTk == SelectedAccount.Id);

                if (existingAccount == null)
                {
                    await _notification_service_fallback("Không tìm thấy tài khoản để chỉnh sửa.", NotificationType.Error);
                    return;
                }

                if (!IsValidEmail(EditEmail))
                {
                    await _notification_service_fallback("Email không hợp lệ!", NotificationType.Warning);
                    return;
                }

                if (existingAccount.Email != EditEmail)
                {
                    if (context.Taikhoans.Any(tk => tk.Email == EditEmail))
                    {
                        await _notification_service_fallback("Email này đã được sử dụng.", NotificationType.Warning);
                        return;
                    }
                    existingAccount.Email = EditEmail;
                }

                if (!string.IsNullOrEmpty(EditPassword))
                {
                    existingAccount.MatKhau = BCrypt.Net.BCrypt.HashPassword(EditPassword);
                }

                if (existingAccount.MaTk == UserSession.Current.AccountId && existingAccount.VaiTro != EditRole)
                {
                    await _notification_service_fallback("Không thể chỉnh sửa vai trò tài khoản của bạn.", NotificationType.Warning);
                    return;
                }
                existingAccount.VaiTro = EditRole;

                if ((EditRole == "Nhân viên" || EditRole == "Admin") && EditSelectedUser != null)
                {
                    if (existingAccount.MaKh != null)
                    {
                        var oldCustomer = context.Khachhangs.Find(existingAccount.MaKh);
                        if (oldCustomer != null)
                        {
                            var newNhanVien = new Nhanvien
                            {
                                HoTenNv = EditFullName,
                                GioiTinh = oldCustomer.GioiTinh,
                                NgaySinh = oldCustomer.NgaySinh,
                                SoDt = oldCustomer.SoDt,
                                Cccd = oldCustomer.Cccd
                            };
                            context.Nhanviens.Add(newNhanVien);
                            context.SaveChanges();

                            existingAccount.MaNv = newNhanVien.MaNv;
                            existingAccount.MaKh = null;
                        }
                    }
                    else
                    {
                        var staff = context.Nhanviens.Find(existingAccount.MaNv);
                        if (staff != null) staff.HoTenNv = EditFullName;
                    }
                }
                else if (EditRole == "Khách hàng" && EditSelectedUser != null)
                {
                    if (existingAccount.MaNv != null)
                    {
                        var oldNhanVien = context.Nhanviens.Find(existingAccount.MaNv);
                        if (oldNhanVien != null)
                        {
                            var newKhachHang = new Khachhang
                            {
                                HoTenKh = oldNhanVien.HoTenNv,
                                GioiTinh = oldNhanVien.GioiTinh,
                                NgaySinh = oldNhanVien.NgaySinh,
                                SoDt = oldNhanVien.SoDt,
                                Cccd = oldNhanVien.Cccd
                            };
                            context.Khachhangs.Add(newKhachHang);
                            context.SaveChanges();

                            existingAccount.MaKh = newKhachHang.MaKh;
                            existingAccount.MaNv = null;
                        }
                    }
                    else
                    {
                        var customer = context.Khachhangs.Find(existingAccount.MaKh);
                        if (customer != null) customer.HoTenKh = EditFullName;
                    }
                }

                context.SaveChanges();

                await _notification_service_fallback("Tài khoản đã được cập nhật thành công!", NotificationType.Information);
                IsEditPopupOpen = false;
                LoadAccounts();
            }
            catch (Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi cập nhật tài khoản: " + ex.Message, NotificationType.Error);
            }
        }

        [RelayCommand]
        public async void DeleteAccount()
        {
            if (SelectedAccount == null)
            {
                await _notification_service_fallback("Vui lòng chọn một tài khoản để xóa.", NotificationType.Warning);
                return;
            }

            bool confirmed = await _notification_service_fallback(
                $"Bạn có chắc chắn muốn xóa tài khoản {SelectedAccount.Email}?",
                NotificationType.Warning,
                isConfirmation: true);

            if (!confirmed)
                return;

            try
            {
                using var context = _createContext();
                var account = context.Taikhoans
                    .FirstOrDefault(tk => tk.Email == SelectedAccount.Email);

                if (account == null)
                {
                    await _notification_service_fallback("Không tìm thấy tài khoản trong cơ sở dữ liệu.", NotificationType.Error);
                    return;
                }

                if (account.MaTk == UserSession.Current.AccountId)
                {
                    await _notification_service_fallback("Không thể xóa tài khoản của bạn.", NotificationType.Warning);
                    return;
                }

                context.Taikhoans.Remove(account);
                context.SaveChanges();

                await _notification_service_fallback("Đã xóa tài khoản thành công!", NotificationType.Information);
                LoadAccounts();
            }
            catch (Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi xóa tài khoản: " + ex.Message, NotificationType.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // prefer _notificationService, fallback to Notification VM if service fails
        private async Task<bool> _notification_service_fallback(string message, NotificationType type, bool isConfirmation = false)
        {
            try
            {
                return await _notificationService.ShowNotificationAsync(message, type, isConfirmation);
            }
            catch
            {
                return await Notification.ShowNotificationAsync(message, type, isConfirmation);
            }
        }
    }
}