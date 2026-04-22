using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public partial class Page52 : Page
    {
        private readonly Admin _currentUser;
        private readonly LuatItem _currentLuat;
        private readonly bool _isEditMode;

        public System.Collections.ObjectModel.ObservableCollection<VehicleFineItem> Fines { get; set; } = new System.Collections.ObjectModel.ObservableCollection<VehicleFineItem>();
        public System.Collections.ObjectModel.ObservableCollection<string> AvailableCategories { get; set; } = new System.Collections.ObjectModel.ObservableCollection<string>();

        // Constructor mặc định
        public Page52()
        {
            InitializeComponent();
            LoadCategories();
        }

        // Constructor chính
        public Page52(LuatItem luat, Admin user = null)
        {
            InitializeComponent();
            _currentUser = user;
            _currentLuat = luat;

            LoadCategories();

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Hoặc _currentUser.HoTen nếu có
                myBell.LoadData(_currentUser as Admin);
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

        // --- CÁC HÀM XỬ LÝ SỰ KIỆN GIAO DIỆN MỚI TỪ PAGE52.XAML ---

        private void DateOverlayBanHanh_Click(object sender, MouseButtonEventArgs e)
        {
            RealDatePickerBanHanh.IsDropDownOpen = true;
        }

        private void RealDatePickerBanHanh_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePickerBanHanh.SelectedDate.HasValue)
            {
                txtNgayBanHanh.Text = RealDatePickerBanHanh.SelectedDate.Value.ToString("dd/MM/yyyy");
            }
        }

        private void RealDatePickerBanHanh_CalendarClosed(object sender, RoutedEventArgs e)
        {
            // Focus lại Overlay để đảm bảo dropdown mượt mà lần sau
            Keyboard.Focus(DateOverlayBanHanh);
        }

        private void DateOverlayHieuLuc_Click(object sender, MouseButtonEventArgs e)
        {
            RealDatePickerHieuLuc.IsDropDownOpen = true;
        }

        private void RealDatePickerHieuLuc_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePickerHieuLuc.SelectedDate.HasValue)
            {
                txtNgayHieuLuc.Text = RealDatePickerHieuLuc.SelectedDate.Value.ToString("dd/MM/yyyy");
            }
        }

        private void RealDatePickerHieuLuc_CalendarClosed(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(DateOverlayHieuLuc);
        }

        private void LoaiXeComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox cb && !string.IsNullOrWhiteSpace(cb.Text))
            {
                string typedVehicle = cb.Text.Trim();

                bool exists = AvailableCategories.Any(c => c.Equals(typedVehicle, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
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

                        lawToSave.LawName = txtTieuDe.Text;

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

                    // Lưu law để lấy LawId (nếu thêm mới)
                    db.SaveChanges(); 

                    // 3. Thêm chi tiết các xe
                    foreach (var m in Fines)
                    {
                        if (string.IsNullOrWhiteSpace(m.LoaiXe)) continue;

                        string loaiXeTrim = m.LoaiXe.Trim();
                        var cat = db.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == loaiXeTrim.ToLower());
                        
                        if (cat == null)
                        {
                            cat = new Category { CategoryName = loaiXeTrim };
                            db.Categories.Add(cat);
                            db.SaveChanges(); // Lấy CategoryId
                        }

                        // Lấy điểm trừ, nếu null ghi 0
                        int t_diem = 0;
                        if (!string.IsNullOrWhiteSpace(m.TruDiem))
                        {
                            var nums = new string(m.TruDiem.Where(char.IsDigit).ToArray());
                            if (!string.IsNullOrEmpty(nums)) int.TryParse(nums, out t_diem);
                        }

                        // Tạo Detail mới
                        var newDetail = new TrafficLawDetail
                        {
                            LawId = lawToSave.LawId,
                            CategoryId = cat.CategoryId,
                            Decree = txtNghiDinh.Text,
                            FineAmount = m.MucPhat,
                            DemeritPoints = t_diem
                        };

                        db.TrafficLawDetails.Add(newDetail);
                    }

                    db.SaveChanges();
                    new CustomMessageBox("Cập nhật thông tin thành công!", "Thông báo").ShowDialog();

                    // Về lại trang danh sách
                    NavigationService?.Navigate(new Page45(_currentUser));
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService?.Navigate(new Page45(_currentUser));
            }
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

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page44(_currentUser));
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page45(_currentUser));
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page46(_currentUser));
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page47(_currentUser));
        }

        private void btnLichSu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page48(_currentUser));
        }

        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page49(_currentUser));
        }
    }
}




