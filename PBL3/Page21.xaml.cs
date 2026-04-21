using PBL3.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PBL3
{
    public partial class Page21 : Page
    {
        // CHỈ NHẬN OFFICER
        private readonly Officer _currentUser;
        private readonly LuatItem _currentLuat;
        private readonly bool _isEditMode;

        public System.Collections.ObjectModel.ObservableCollection<VehicleFineItem> Fines { get; set; } = new System.Collections.ObjectModel.ObservableCollection<VehicleFineItem>();
        public System.Collections.ObjectModel.ObservableCollection<string> AvailableCategories { get; set; } = new System.Collections.ObjectModel.ObservableCollection<string>();

        // Constructor mặc định
        public Page21()
        {
            InitializeComponent();
            LoadCategories();
        }

        // Constructor chính nhận dữ liệu
        public Page21(LuatItem luat, Officer user = null)
        {
            InitializeComponent();
            _currentUser = user;
            _currentLuat = luat;

            LoadCategories();

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }

            // NẾU CÓ TRUYỀN LUẬT SANG -> CHẾ ĐỘ CHỈNH SỬA
            if (_currentLuat != null)
            {
                _isEditMode = true;
                txtTieuDe.Text = _currentLuat.TenLoi;
                txtNghiDinh.Text = _currentLuat.CanCu;

                if (DateTime.TryParseExact(_currentLuat.NgayBanHanh, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime pIssue))
                    txtNgayBanHanh.Text = pIssue.ToString("dd/MM/yyyy");

                if (DateTime.TryParseExact(_currentLuat.NgayHieuLuc, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime pEffective))
                    txtNgayHieuLuc.Text = pEffective.ToString("dd/MM/yyyy");

                if (!string.IsNullOrWhiteSpace(_currentLuat.PhatTienXeMay))
                    Fines.Add(new VehicleFineItem { LoaiXe = "Xe máy", MucPhat = _currentLuat.PhatTienXeMay, TruDiem = _currentLuat.TruDiem });
                
                if (!string.IsNullOrWhiteSpace(_currentLuat.PhatTienOto))
                    Fines.Add(new VehicleFineItem { LoaiXe = "Ô tô", MucPhat = _currentLuat.PhatTienOto, TruDiem = _currentLuat.TruDiem, CanRemove = true });
            }
            // NẾU KHÔNG CÓ -> CHẾ ĐỘ THÊM MỚI
            else
            {
                _isEditMode = false;
            }

            if (Fines.Count == 0)
                Fines.Add(new VehicleFineItem());

            // Ensure first item cannot be removed, others can
            for (int i = 0; i < Fines.Count; i++)
            {
                Fines[i].CanRemove = i > 0;
            }

            icPhuongTien.ItemsSource = Fines;
        }

        private void LoadCategories()
        {
            try
            {
                using (var db = new Models.TrafficSafetyDBContext())
                {
                    var categories = db.Categories.Select(c => c.CategoryName).ToList();
                    AvailableCategories.Clear();
                    foreach (var cat in categories)
                    {
                        AvailableCategories.Add(cat);
                    }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi tải danh sách phương tiện: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        private void LoaiXeComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && !string.IsNullOrWhiteSpace(cb.Text))
            {
                string typedVehicle = cb.Text.Trim();

                // See if it is already in our loaded categories list (case-insensitive)
                bool exists = AvailableCategories.Any(c => c.Equals(typedVehicle, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    // Show confirmation window to add it to database
                    var confirmResult = System.Windows.MessageBox.Show(
                        $"Loại xe '{typedVehicle}' chưa có trong hệ thống.\nBạn có muốn thêm loại xe này vào danh sách không?",
                        "Xác nhận thêm mới",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (confirmResult == System.Windows.MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (var db = new Models.TrafficSafetyDBContext())
                            {
                                var newCat = new Models.Category { CategoryName = typedVehicle };
                                db.Categories.Add(newCat);
                                db.SaveChanges();
                                
                                AvailableCategories.Add(newCat.CategoryName);
                            }
                        }
                        catch (Exception ex)
                        {
                            new CustomMessageBox("Lỗi khi thêm mới loại xe: " + ex.Message, "Lỗi").ShowDialog();
                        }
                    }
                    else
                    {
                        // Revert text to empty if they chose 'No'
                        cb.Text = "";
                    }
                }
            }
        }

        private void btnThemPhuongTien_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new VehicleFineItem { CanRemove = true };
            Fines.Add(newItem);
        }

        private void btnXoaPhuongTien_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is VehicleFineItem item)
            {
                Fines.Remove(item);
            }
        }

        // --- NÚT LƯU LUẬT (DÙNG ENTITY FRAMEWORK) ---
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTieuDe.Text))
            {
                new CustomMessageBox("Vui lòng nhập Tiêu đề luật.", "Thông báo").ShowDialog();
                return;
            }

            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    TrafficLaw lawToSave;

                    // 1. NẾU LÀ CHỈNH SỬA
                    if (_isEditMode && _currentLuat != null)
                    {
                        lawToSave = db.TrafficLaws.FirstOrDefault(l => l.LawId == _currentLuat.LawId);
                        if (lawToSave == null)
                        {
                            new CustomMessageBox("Không tìm thấy luật để cập nhật.", "Lỗi").ShowDialog();
                            return;
                        }

                        // Cập nhật tên luật
                        lawToSave.LawName = txtTieuDe.Text;

                        // Xóa sạch các chi tiết phạt cũ (Ô tô, Xe máy) để ghi đè cái mới
                        var oldDetails = db.TrafficLawDetails.Where(d => d.LawId == lawToSave.LawId);
                        db.TrafficLawDetails.RemoveRange(oldDetails);
                    }
                    // 2. NẾU LÀ THÊM MỚI
                    else
                    {
                        lawToSave = new TrafficLaw
                        {
                            LawName = txtTieuDe.Text
                        };
                        db.TrafficLaws.Add(lawToSave);
                    }

                    // Lưu thay đổi vào bảng TRAFFIC_LAWS trước để lấy LawId (Mã luật tự tăng)
                    db.SaveChanges();

                    // 3. THÊM CHI TIẾT MỨC PHẠT VÀO BẢNG TRAFFIC_LAW_DETAILS
                    foreach (var fine in Fines)
                    {
                        if (!string.IsNullOrWhiteSpace(fine.LoaiXe) || !string.IsNullOrWhiteSpace(fine.MucPhat) || !string.IsNullOrWhiteSpace(fine.TruDiem))
                        {
                            db.TrafficLawDetails.Add(CreateLawDetail(db, lawToSave.LawId, fine.LoaiXe, fine.MucPhat, fine.TruDiem));
                        }
                    }

                    // Lưu tất cả các thay đổi
                    db.SaveChanges();

                    new CustomMessageBox("Đã lưu thông tin luật thành công!", "Thông báo").ShowDialog();

                    // Trở về trang trước
                    if (NavigationService.CanGoBack) NavigationService.GoBack();
                    else NavigationService.Navigate(new Page13(_currentUser));
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi lưu vào CSDL: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        // --- HÀM TẠO ĐỐI TƯỢNG CHI TIẾT LUẬT (Hàm này nãy Nhi bị rớt mất nè) ---
        private TrafficLawDetail CreateLawDetail(TrafficSafetyDBContext db, int lawId, string vehicleType, string fineAmount, string truDiemTxt)
        {
            // Parse điểm trừ
            int demeritPoints = 0;
            if (int.TryParse(string.Join("", (truDiemTxt ?? "").Where(char.IsDigit)), out int points))
            {
                demeritPoints = points;
            }

            // Parse Ngày (An toàn)
            DateTime? issueDate = null;
            if (DateTime.TryParseExact(txtNgayBanHanh.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedIssue))
                issueDate = parsedIssue;

            DateTime? effectiveDate = null;
            if (DateTime.TryParseExact(txtNgayHieuLuc.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEff))
                effectiveDate = parsedEff;

            int? categoryId = null;
            if (!string.IsNullOrWhiteSpace(vehicleType))
            {
                var cat = db.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == vehicleType.ToLower());
                if (cat != null)
                {
                    categoryId = cat.CategoryId;
                }
                else
                {
                    var newCat = new Category { CategoryName = vehicleType };
                    db.Categories.Add(newCat);
                    db.SaveChanges();
                    categoryId = newCat.CategoryId;
                }
            }

            return new TrafficLawDetail
            {
                LawId = lawId,
                CategoryId = categoryId,
                FineAmount = fineAmount,
                DemeritPoints = demeritPoints,
                Decree = txtNghiDinh.Text ?? string.Empty,
                IssueDate = issueDate,
                EffectiveDate = effectiveDate
            };
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG BÊN DƯỚI & SIDEBAR ---
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page13(_currentUser));
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page16(_currentUser));

        private void RealDatePickerBanHanh_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePickerBanHanh.SelectedDate is DateTime selectedDate)
                txtNgayBanHanh.Text = selectedDate.ToString("dd/MM/yyyy");
        }

        private void DateOverlayBanHanh_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DateOverlayBanHanh.IsHitTestVisible = false;
            RealDatePickerBanHanh.IsHitTestVisible = true;
            RealDatePickerBanHanh.Opacity = 0.01;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                RealDatePickerBanHanh.Focus();
                RealDatePickerBanHanh.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RealDatePickerBanHanh_CalendarClosed(object sender, RoutedEventArgs e)
        {
            RealDatePickerBanHanh.IsHitTestVisible = false;
            RealDatePickerBanHanh.Opacity = 0;
            DateOverlayBanHanh.IsHitTestVisible = true;
        }

        private void RealDatePickerHieuLuc_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePickerHieuLuc.SelectedDate is DateTime selectedDate)
                txtNgayHieuLuc.Text = selectedDate.ToString("dd/MM/yyyy");
        }

        private void DateOverlayHieuLuc_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DateOverlayHieuLuc.IsHitTestVisible = false;
            RealDatePickerHieuLuc.IsHitTestVisible = true;
            RealDatePickerHieuLuc.Opacity = 0.01;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                RealDatePickerHieuLuc.Focus();
                RealDatePickerHieuLuc.IsDropDownOpen = true;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RealDatePickerHieuLuc_CalendarClosed(object sender, RoutedEventArgs e)
        {
            RealDatePickerHieuLuc.IsHitTestVisible = false;
            RealDatePickerHieuLuc.Opacity = 0;
            DateOverlayHieuLuc.IsHitTestVisible = true;
        }
    }

    public class VehicleFineItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _loaiXe = "";
        public string LoaiXe { get => _loaiXe; set { _loaiXe = value; OnPropertyChanged(nameof(LoaiXe)); } }

        private string _mucPhat = "";
        public string MucPhat { get => _mucPhat; set { _mucPhat = value; OnPropertyChanged(nameof(MucPhat)); } }

        private string _truDiem = "";
        public string TruDiem { get => _truDiem; set { _truDiem = value; OnPropertyChanged(nameof(TruDiem)); } }

        private bool _canRemove = false;
        public bool CanRemove 
        { 
            get => _canRemove; 
            set 
            { 
                _canRemove = value; 
                OnPropertyChanged(nameof(CanRemove)); 
                OnPropertyChanged(nameof(RemoveBtnVisibility)); 
            } 
        }

        public System.Windows.Visibility RemoveBtnVisibility => CanRemove ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) { PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name)); }
    }
}