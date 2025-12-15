using AirTicketSalesManagement.Data;
using AirTicketSalesManagement.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using AirTicketSalesManagement.Services.DbContext;
using AirTicketSalesManagement.Services.Notification;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace AirTicketSalesManagement.ViewModel.Admin
{
    public partial class FlightManagementViewModel : BaseViewModel
    {
        // --- Dependencies (injected for testability) ---
        private readonly IAirTicketDbContextService _dbContextService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string diemDi;

        [ObservableProperty]
        private string diemDen;

        [ObservableProperty]
        private string soHieuCB;

        [ObservableProperty]
        private string trangThai;

        [ObservableProperty]
        private string hangHangKhong;

        [ObservableProperty]
        private ObservableCollection<Chuyenbay> flights = new();

        [ObservableProperty]
        private Chuyenbay selectedFlight;

        [ObservableProperty]
        private ObservableCollection<string> sanBayList = new();

        // Danh sách dùng để binding cho điểm đi (lọc bỏ điểm đến)
        public ObservableCollection<string> DiemDiList =>
            new(SanBayList.Where(s => s != DiemDen));

        // Danh sách dùng để binding cho điểm đến (lọc bỏ điểm đi)
        public ObservableCollection<string> DiemDenList =>
            new(SanBayList.Where(s => s != DiemDi));

        partial void OnDiemDiChanged(string value)
        {
            OnPropertyChanged(nameof(DiemDenList));
        }

        partial void OnDiemDenChanged(string value)
        {
            OnPropertyChanged(nameof(DiemDiList));
        }

        // Them chuyen bay
        [ObservableProperty]
        private string addDiemDi;
        [ObservableProperty]
        private string addDiemDen;
        [ObservableProperty]
        private string addSoHieuCB;
        [ObservableProperty]
        private string addHangHangKhong;
        [ObservableProperty]
        private string addTTKhaiThac;
        [ObservableProperty]
        private ObservableCollection<SBTG> danhSachSBTG = new();

        public ObservableCollection<string> AddDiemDiList =>
            new(SanBayList.Where(s => s != AddDiemDen));

        public ObservableCollection<string> AddDiemDenList =>
            new(SanBayList.Where(s => s != AddDiemDi));

        public ObservableCollection<string> SBTGList =>
            new(SanBayList.Where(s => s != AddDiemDi && s != AddDiemDen));

        partial void OnAddDiemDiChanged(string value)
        {
            OnPropertyChanged(nameof(AddDiemDenList));
            OnPropertyChanged(nameof(SBTGList));
            CapNhatSBTGList();
        }

        partial void OnAddDiemDenChanged(string value)
        {
            OnPropertyChanged(nameof(AddDiemDiList));
            OnPropertyChanged(nameof(SBTGList));
            CapNhatSBTGList();
        }

        [ObservableProperty]
        private bool isAddPopupOpen = false;

        //Sua chuyen bay
        [ObservableProperty]
        private string editDiemDi;
        [ObservableProperty]
        private string editDiemDen;
        [ObservableProperty]
        private string editSoHieuCB;
        [ObservableProperty]
        private string editHangHangKhong;
        [ObservableProperty]
        private string editTTKhaiThac;
        [ObservableProperty]
        private bool isEditPopupOpen = false;

        public ObservableCollection<string> EditDiemDiList =>
            new(SanBayList.Where(s => s != EditDiemDen));

        public ObservableCollection<string> EditDiemDenList =>
            new(SanBayList.Where(s => s != EditDiemDi));

        public ObservableCollection<string> EditSBTGList =>
            new(SanBayList.Where(s => s != EditDiemDen && s != EditDiemDi));

        partial void OnEditDiemDiChanged(string value)
        {
            OnPropertyChanged(nameof(AddDiemDenList));
            OnPropertyChanged(nameof(SBTGList));
            CapNhatSBTGList();
        }

        partial void OnEditDiemDenChanged(string value)
        {
            OnPropertyChanged(nameof(AddDiemDiList));
            OnPropertyChanged(nameof(SBTGList));
            CapNhatSBTGList();
        }
        public NotificationViewModel Notification { get; }

        // Parameterless ctor kept for runtime compatibility (concrete services)
        public FlightManagementViewModel()
            : this(new AirTicketDbService(), new NotificationService(new NotificationViewModel()))
        {
        }

        // DI ctor - use in unit tests to inject fakes/mocks
        public FlightManagementViewModel(IAirTicketDbContextService dbContextService, INotificationService notificationService)
        {
            _dbContextService = dbContextService ?? throw new System.ArgumentNullException(nameof(dbContextService));
            _notificationService = notificationService;

            // keep Notification property for UI binding if the view binds to it
            Notification = (notificationService as NotificationService)?.ViewModel
                       ?? new NotificationViewModel();

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                LoadSanBay();
                LoadFlights();
            }
        }

        public void LoadSanBay()
        {
            using var context = _dbContextService.CreateDbContext();
            var danhSach = context.Sanbays
                        .AsEnumerable()
                        .Select(sb => $"{sb.ThanhPho} ({sb.MaSb}), {sb.QuocGia}")
                        .OrderBy(display => display)
                        .ToList();
            SanBayList = new ObservableCollection<string>(danhSach);
        }

        public void LoadFlights()
        {
            using var context = _dbContextService.CreateDbContext();
            var danhSach = context.Chuyenbays
                .Include(cb => cb.SbdiNavigation)
                .Include(cb => cb.SbdenNavigation)
                .Include(cb => cb.Sanbaytrunggians)
                    .ThenInclude(sbtg => sbtg.MaSbtgNavigation)
                .AsEnumerable()
                .Select(cb =>
                {
                    cb.SbdiNavigation ??= new Sanbay();
                    cb.SbdenNavigation ??= new Sanbay();
                    return cb;
                })
                .ToList();

            Flights = new ObservableCollection<Chuyenbay>(danhSach);
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void Refresh()
        {
            LoadFlights();
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void ClearSearch()
        {
            DiemDi = string.Empty;
            DiemDen = string.Empty;
            SoHieuCB = string.Empty;
            TrangThai = string.Empty;
            HangHangKhong = string.Empty;
            LoadFlights();
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void Search()
        {
            Flights.Clear();

            using var context = _dbContextService.CreateDbContext();
            var query = context.Chuyenbays
                .Include(cb => cb.SbdiNavigation)
                .Include(cb => cb.SbdenNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(DiemDi))
            {
                var maSBDi = ExtractMaSB(DiemDi);
                query = query.Where(cb => cb.SbdiNavigation.MaSb == maSBDi);
            }

            if (!string.IsNullOrWhiteSpace(DiemDen))
            {
                var maSBDen = ExtractMaSB(DiemDen);
                query = query.Where(cb => cb.SbdenNavigation.MaSb == maSBDen);
            }

            if (!string.IsNullOrWhiteSpace(SoHieuCB))
            {
                query = query.Where(cb => cb.SoHieuCb.Contains(SoHieuCB));
            }

            if (!string.IsNullOrWhiteSpace(TrangThai) && TrangThai != "Tất cả")
            {
                query = query.Where(cb => cb.TtkhaiThac == TrangThai);
            }

            if (!string.IsNullOrWhiteSpace(HangHangKhong) && HangHangKhong != "Tất cả")
            {
                query = query.Where(cb => cb.HangHangKhong == HangHangKhong);
            }

            foreach (var cb in query.ToList())
            {
                Flights.Add(cb);
            }
        }

        private string ExtractMaSB(string displayString)
        {
            if (string.IsNullOrWhiteSpace(displayString)) return "";
            int start = displayString.IndexOf('(');
            int end = displayString.IndexOf(')');
            if (start >= 0 && end > start)
                return displayString.Substring(start + 1, end - start - 1);
            return displayString;
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void AddFlight()
        {
            ResetAddField();
            IsAddPopupOpen = true;
        }
        [ExcludeFromCodeCoverage]
        private void ResetAddField()
        {
            AddDiemDen = string.Empty;
            AddDiemDi = string.Empty;
            AddSoHieuCB = string.Empty;
            AddHangHangKhong = string.Empty;
            AddTTKhaiThac = string.Empty;
            DanhSachSBTG = new ObservableCollection<SBTG>();
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void CancelAddFlight()
        {
            IsAddPopupOpen = false;
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void CloseAdd()
        {
            IsAddPopupOpen = false;
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async void AddIntermediateAirport()
        {
            using var context = _dbContextService.CreateDbContext();
            int soSBTG = 0;
            var quyDinh = context.Quydinhs.FirstOrDefault();
            if (quyDinh != null)
            {
                soSBTG = quyDinh.SoSanBayTgtoiDa.GetValueOrDefault();
            }
            if (DanhSachSBTG.Count >= soSBTG)
            {
                await _notification_service_fallback($"Số sân bay trung gian tối đa là: {soSBTG}", NotificationType.Warning);
                return;
            }

            try
            {
                var sbtg = new SBTG()
                {
                    STT = DanhSachSBTG.Count + 1,
                    MaSBTG = string.Empty,
                    ThoiGianDung = 0,
                    GhiChu = string.Empty,
                    SbtgList = new ObservableCollection<string>(SBTGList),
                    OnMaSBTGChangedCallback = CapNhatSBTGList
                };

                DanhSachSBTG.Add(sbtg);
                CapNhatSBTGList();
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi thêm sân bay trung gian: {ex.Message}", NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async void RemoveIntermediateAirport(SBTG addSBTG)
        {
            try
            {
                if (addSBTG == null)
                {
                    await _notification_service_fallback("Không tìm thấy sân bay trung gian để xóa!", NotificationType.Warning);
                    return;
                }

                bool confirmed = await _notification_service_fallback(
                    $"Bạn có chắc chắn muốn xóa sân bay trung gian thứ {addSBTG.STT}?",

                    NotificationType.Warning,
                    isConfirmation: true);

                if (confirmed)
                {
                    int removedSTT = addSBTG.STT;
                    DanhSachSBTG.Remove(addSBTG);
                    UpdateSTTAfterRemoval(removedSTT);
                }
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi xóa sân bay trung gian: {ex.Message}", NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        private void UpdateSTTAfterRemoval(int removedSTT)
        {
            try
            {
                foreach (var airport in DanhSachSBTG.Where(a => a.STT > removedSTT))
                {
                    airport.STT--;
                }

                var sortedList = DanhSachSBTG.OrderBy(a => a.STT).ToList();
                DanhSachSBTG.Clear();

                foreach (var airport in sortedList)
                {
                    DanhSachSBTG.Add(airport);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật STT: {ex.Message}");
            }
        }

        [ExcludeFromCodeCoverage]
        private void CapNhatSBTGList()
        {
            if (DanhSachSBTG == null || DanhSachSBTG.Count == 0)
            {
                return;
            }

            var danhSachCoBan = SanBayList
                .Where(s => s != AddDiemDi && s != AddDiemDen)
                .ToList();

            foreach (var item in DanhSachSBTG)
            {
                var daChon = DanhSachSBTG
                    .Where(x => x != item && !string.IsNullOrWhiteSpace(x.MaSBTG))
                    .Select(x => x.MaSBTG)
                    .ToList();

                var danhSachLoc = danhSachCoBan
                    .Where(sbtg => !daChon.Contains(sbtg))
                    .ToList();

                item.SbtgList = new ObservableCollection<string>(danhSachLoc);
            }
        }

        [RelayCommand]
        public async void SaveFlight()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddDiemDi) || string.IsNullOrWhiteSpace(AddDiemDen) ||
                    string.IsNullOrWhiteSpace(AddSoHieuCB) || string.IsNullOrWhiteSpace(AddHangHangKhong) ||
                    string.IsNullOrWhiteSpace(AddTTKhaiThac))
                {
                    await _notification_service_fallback("Vui lòng điền đầy đủ thông tin chuyến bay.", NotificationType.Warning);
                    return;
                }

                using var context = _dbContextService.CreateDbContext();

                bool isDuplicate = context.Chuyenbays.Any(cb => cb.SoHieuCb == AddSoHieuCB);
                if (isDuplicate)
                {
                    await _notification_service_fallback("Số hiệu chuyến bay đã tồn tại. Vui lòng nhập số hiệu khác.", NotificationType.Warning);
                    return;
                }

                int thoiGianDungMin = 0;
                int thoiGianDungMax = int.MaxValue;
                var quyDinh = context.Quydinhs.FirstOrDefault();

                if (quyDinh != null)
                {
                    thoiGianDungMin = quyDinh.TgdungMin.GetValueOrDefault();
                    thoiGianDungMax = quyDinh.TgdungMax.GetValueOrDefault(int.MaxValue);
                }

                foreach (var sbtg in DanhSachSBTG)
                {
                    if (!string.IsNullOrWhiteSpace(sbtg.MaSBTG))
                    {
                        if (thoiGianDungMin > sbtg.ThoiGianDung)
                        {
                            await _notification_service_fallback($"Thời gian dừng tối thiểu là: {thoiGianDungMin} phút", NotificationType.Warning);
                            return;
                        }
                        else if (thoiGianDungMax < sbtg.ThoiGianDung)
                        {
                            await _notification_service_fallback($"Thời gian dừng tối đa là: {thoiGianDungMax} phút", NotificationType.Warning);
                            return;
                        }
                    }
                }

                var newFlight = new Chuyenbay
                {
                    SoHieuCb = AddSoHieuCB,
                    SbdiNavigation = context.Sanbays.FirstOrDefault(sb => sb.MaSb == ExtractMaSB(AddDiemDi)),
                    SbdenNavigation = context.Sanbays.FirstOrDefault(sb => sb.MaSb == ExtractMaSB(AddDiemDen)),
                    HangHangKhong = AddHangHangKhong,
                    TtkhaiThac = AddTTKhaiThac,
                };

                context.Chuyenbays.Add(newFlight);
                context.SaveChanges();

                foreach (var sbtg in DanhSachSBTG)
                {
                    if (!string.IsNullOrWhiteSpace(sbtg.MaSBTG))
                    {
                        var sb = context.Sanbays.FirstOrDefault(s => s.MaSb == ExtractMaSB(sbtg.MaSBTG));
                        if (sb != null)
                        {
                            var sbtg1 = new Sanbaytrunggian
                            {
                                Stt = sbtg.STT,
                                SoHieuCb = newFlight.SoHieuCb,
                                MaSbtg = sb.MaSb,
                                ThoiGianDung = sbtg.ThoiGianDung,
                                GhiChu = sbtg.GhiChu
                            };
                            context.Sanbaytrunggians.Add(sbtg1);
                        }
                    }
                }

                context.SaveChanges();
                await _notification_service_fallback("Chuyến bay đã được thêm thành công!", NotificationType.Information);
                IsAddPopupOpen = false;
                LoadFlights();
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi thêm chuyến bay: " + ex.Message, NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async void EditFlight()
        {
            if (SelectedFlight == null)
            {
                await _notification_service_fallback("Vui lòng chọn một chuyến bay để chỉnh sửa.", NotificationType.Warning);
                return;
            }

            using var context = _dbContextService.CreateDbContext();
            var existingFlight = context.Chuyenbays
                .Include(cb => cb.Lichbays)
                .Include(cb => cb.Sanbaytrunggians)
                .FirstOrDefault(cb => cb.SoHieuCb == SelectedFlight.SoHieuCb);

            if (existingFlight == null)
            {
                await _notification_service_fallback("Không tìm thấy chuyến bay để chỉnh sửa.", NotificationType.Error);
                return;
            }

            if (existingFlight.Lichbays.Any())
            {
                await _notification_service_fallback("Không thể chỉnh sửa chuyến bay đã có lịch bay.", NotificationType.Warning);
                return;
            }

            DanhSachSBTG = new ObservableCollection<SBTG>();
            ResetEditField();
            IsEditPopupOpen = true;
        }

        [ExcludeFromCodeCoverage]
        private void ResetEditField()
        {
            EditDiemDi = SelectedFlight?.SbdiNavigation != null
            ? $"{SelectedFlight.SbdiNavigation.ThanhPho} ({SelectedFlight.SbdiNavigation.MaSb}), {SelectedFlight.SbdiNavigation.QuocGia}"
            : string.Empty;
            EditDiemDen = SelectedFlight?.SbdenNavigation != null
            ? $"{SelectedFlight.SbdenNavigation.ThanhPho} ({SelectedFlight.SbdenNavigation.MaSb}), {SelectedFlight.SbdenNavigation.QuocGia}"
            : string.Empty;
            EditHangHangKhong = SelectedFlight?.HangHangKhong ?? string.Empty;
            EditSoHieuCB = SelectedFlight?.SoHieuCb ?? string.Empty;
            EditTTKhaiThac = SelectedFlight?.TtkhaiThac ?? string.Empty;
            foreach (var sbtg in SelectedFlight?.Sanbaytrunggians ?? Enumerable.Empty<Sanbaytrunggian>())
            {
                DanhSachSBTG.Add(new SBTG
                {
                    STT = sbtg.Stt,
                    MaSBTG = $"{sbtg.MaSbtgNavigation.ThanhPho} ({sbtg.MaSbtgNavigation.MaSb}), {sbtg.MaSbtgNavigation.QuocGia}",
                    ThoiGianDung = sbtg.ThoiGianDung.GetValueOrDefault(),
                    GhiChu = sbtg.GhiChu,
                    SbtgList = new ObservableCollection<string>(EditSBTGList),
                    OnMaSBTGChangedCallback = CapNhatSBTGList
                });
            }
            CapNhatSBTGList();
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async void DeleteFlight()
        {
            if (SelectedFlight == null)
            {
                await _notification_service_fallback("Vui lòng chọn một chuyến bay để xóa.", NotificationType.Warning);
                return;
            }

            bool confirmed = await _notification_service_fallback(
                $"Bạn có chắc chắn muốn xóa chuyến bay {SelectedFlight.SoHieuCb}?",
                NotificationType.Warning,
                isConfirmation: true);

            if (!confirmed)
                return;

            try
            {
                using var context = _dbContextService.CreateDbContext();
                var flight = context.Chuyenbays
                    .Include(cb => cb.Lichbays)
                    .Include(cb => cb.Sanbaytrunggians)
                    .FirstOrDefault(cb => cb.SoHieuCb == SelectedFlight.SoHieuCb);

                if (flight == null)
                {
                    await _notification_service_fallback("Không tìm thấy chuyến bay trong hệ thống.", NotificationType.Error);
                    return;
                }

                if (flight.Lichbays.Any())
                {
                    await _notification_service_fallback("Không thể xóa chuyến bay đã có lịch bay.", NotificationType.Warning);
                    return;
                }

                context.Sanbaytrunggians.RemoveRange(flight.Sanbaytrunggians);
                context.Chuyenbays.Remove(flight);
                context.SaveChanges();

                await _notification_service_fallback("Đã xóa chuyến bay thành công!", NotificationType.Information);
                LoadFlights();
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi xóa chuyến bay: " + ex.Message, NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void CancelEditFlight()
        {
            IsEditPopupOpen = false;
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public void CloseEdit()
        {
            IsEditPopupOpen = false;
        }

        [RelayCommand]
        public async void SaveEditFlight()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditDiemDi) || string.IsNullOrWhiteSpace(EditDiemDen) ||
                    string.IsNullOrWhiteSpace(EditSoHieuCB) || string.IsNullOrWhiteSpace(EditHangHangKhong) ||
                    string.IsNullOrWhiteSpace(EditTTKhaiThac))
                {
                    await _notification_service_fallback("Vui lòng điền đầy đủ thông tin chuyến bay.", NotificationType.Warning);
                    return;
                }

                using var context = _dbContextService.CreateDbContext();
                var existingFlight = context.Chuyenbays
                    .Include(cb => cb.Lichbays)
                    .Include(cb => cb.Sanbaytrunggians)
                    .FirstOrDefault(cb => cb.SoHieuCb == EditSoHieuCB);

                

                existingFlight.SbdiNavigation = context.Sanbays.FirstOrDefault(sb => sb.MaSb == ExtractMaSB(EditDiemDi));
                existingFlight.SbdenNavigation = context.Sanbays.FirstOrDefault(sb => sb.MaSb == ExtractMaSB(EditDiemDen));
                existingFlight.Sbdi = existingFlight.SbdiNavigation?.MaSb;
                existingFlight.Sbden = existingFlight.SbdenNavigation?.MaSb;
                existingFlight.HangHangKhong = EditHangHangKhong;
                existingFlight.TtkhaiThac = EditTTKhaiThac;

                context.Sanbaytrunggians.RemoveRange(existingFlight.Sanbaytrunggians);

                foreach (var sbtg in DanhSachSBTG)
                {
                    if (!string.IsNullOrWhiteSpace(sbtg.MaSBTG))
                    {
                        var sb = context.Sanbays.FirstOrDefault(s => s.MaSb == ExtractMaSB(sbtg.MaSBTG));
                        if (sb != null)
                        {
                            var sbtgEntity = new Sanbaytrunggian
                            {
                                Stt = sbtg.STT,
                                SoHieuCb = existingFlight.SoHieuCb,
                                MaSbtg = sb.MaSb,
                                ThoiGianDung = sbtg.ThoiGianDung,
                                GhiChu = sbtg.GhiChu
                            };
                            context.Sanbaytrunggians.Add(sbtgEntity);
                        }
                    }
                }

                context.SaveChanges();

                await _notification_service_fallback("Chuyến bay đã được cập nhật thành công!", NotificationType.Information);
                IsEditPopupOpen = false;
                LoadFlights();
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback("Đã xảy ra lỗi khi cập nhật chuyến bay: " + ex.Message, NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async Task EditIntermediateAirportAsync()
        {
            using var context = _dbContextService.CreateDbContext();
            int soSBTG = 0;
            var quyDinh = context.Quydinhs.FirstOrDefault();

            if (quyDinh != null)
            {
                soSBTG = quyDinh.SoSanBayTgtoiDa.GetValueOrDefault();
            }
            if (DanhSachSBTG.Count >= soSBTG)
            {
                await _notification_service_fallback($"Số sân bay trung gian tối đa là: {soSBTG}", NotificationType.Warning);
                return;
            }

            try
            {
                var sbtg = new SBTG()
                {
                    STT = DanhSachSBTG.Count + 1,
                    MaSBTG = string.Empty,
                    ThoiGianDung = 0,
                    GhiChu = string.Empty,
                    SbtgList = new ObservableCollection<string>(SBTGList),
                    OnMaSBTGChangedCallback = CapNhatSBTGList
                };

                DanhSachSBTG.Add(sbtg);
                CapNhatSBTGList();
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi thêm sân bay trung gian: {ex.Message}", NotificationType.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        [RelayCommand]
        public async void RemoveEditIntermediateAirport(SBTG editSBTG)
        {
            try
            {
                if (editSBTG == null)
                {
                    await _notification_service_fallback("Không tìm thấy sân bay trung gian để xóa!", NotificationType.Warning);
                    return;
                }

                bool confirmed = await _notification_service_fallback(
                    $"Bạn có chắc chắn muốn xóa sân bay trung gian thứ {editSBTG.STT}?",
                    NotificationType.Warning,
                    isConfirmation: true);

                if (confirmed)
                {
                    int removedSTT = editSBTG.STT;
                    DanhSachSBTG.Remove(editSBTG);
                    UpdateSTTAfterRemoval(removedSTT);
                }
            }
            catch (System.Exception ex)
            {
                await _notification_service_fallback($"Lỗi khi xóa sân bay trung gian: {ex.Message}", NotificationType.Error);
            }
        }

        [ObservableProperty]
        private bool isSearchExpanded = true;

        [ObservableProperty]
        private double searchContentHeight = double.NaN;

        [RelayCommand]
        private void ToggleSearch()
        {
            IsSearchExpanded = !IsSearchExpanded;
        }

        // Prefer _notificationService, fallback to Notification VM if service fails
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