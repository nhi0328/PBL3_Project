using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBL3
{
    public class LuatItem
    {
        public string TenLoi { get; set; }
        public string PhatTienOto { get; set; }
        public string PhatTienXeMay { get; set; }
        public string TruDiem { get; set; }
        public string CanCu { get; set; }
        public string NgayBanHanh { get; set; }
        public string NgayHieuLuc { get; set; }

        // Các Biến này sẽ giúp giao diện Tự động Ẩn dẨng ch? n?u luật �� không �p dẨng cho xe ��
        public bool HasPhatTienOto => !string.IsNullOrEmpty(PhatTienOto);
        public bool HasPhatTienXeMay => !string.IsNullOrEmpty(PhatTienXeMay);
        public bool HasTruDiem => !string.IsNullOrEmpty(TruDiem) && !TruDiem.StartsWith("Trừ 0", StringComparison.OrdinalIgnoreCase) && !TruDiem.StartsWith("Tr\u1EEB 0", StringComparison.OrdinalIgnoreCase);
    }

    public partial class Page13 : Page
    {
        private ObservableCollection<LuatItem> lstLuat;

        // Constructor mặc định
        public Page13()
        {
            InitializeComponent();
            LoadData();
        }

        private User _currentUser;

        public Page13(User user) : this()
        {
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }
        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page());
        }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page9());
        }
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page10());
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            // PHÂN QUYỀN HIỂN THỊ MENU

            if (_currentUser is Customer)
            {
                // Công dân: Ẩn Các n�t chuyẨn giao diện v� thanh kẻ phụ



            }
            else if (_currentUser is Officer)
            {
                // Cán bộ: Được xem giao diện Khách hàng



            }
            else if (_currentUser is Admin)
            {
                // Quản trị viên: HiẨn Tất cả Các lựa chọn để ki?m tra



            }

            // Mở Menu
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }
        public Page13(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void LoadData()
        {
            try
            {
                Dictionary<string, LuatItem> groupedData = new Dictionary<string, LuatItem>();

                using (TrafficSafetyDBContext db = new TrafficSafetyDBContext())
                {
                    var trafficLaws = db.Set<TrafficLaw>().ToList();

                    foreach (var law in trafficLaws)
                    {
                        string tenLoi = law.ViolationDescription ?? string.Empty;
                        string loaiXe = law.VehicleType ?? string.Empty;
                        string mucPhat = law.FineRange ?? string.Empty;
                        string truDiem = law.PointsDeducted ?? string.Empty;

                        if (!groupedData.ContainsKey(tenLoi))
                        {
                            groupedData[tenLoi] = new LuatItem
                            {
                                TenLoi = tenLoi,
                                TruDiem = truDiem,
                                CanCu = law.LawReference ?? string.Empty,
                                NgayBanHanh = law.IssueDate ?? string.Empty,
                                NgayHieuLuc = law.EffectiveDate ?? string.Empty
                            };
                        }

                        if (loaiXe.Contains("Ô tô", StringComparison.OrdinalIgnoreCase) || loaiXe.Contains("Ô tô", StringComparison.OrdinalIgnoreCase))
                        {
                            groupedData[tenLoi].PhatTienOto = mucPhat;
                        }
                        else if (loaiXe.Contains("Xe máy", StringComparison.OrdinalIgnoreCase) || loaiXe.Contains("Xe máy", StringComparison.OrdinalIgnoreCase))
                        {
                            groupedData[tenLoi].PhatTienXeMay = mucPhat;
                        }
                    }
                }

                lstLuat = new ObservableCollection<LuatItem>(groupedData.Values);
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi k?t n?i CSDL: " + ex.Message);
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        // Chuyển trang Lập biên bản vi phạm
        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        //Chuyển trang QuẨn l? tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        // Xử lý sẽ kiẨn n�t Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterLaws();
        }

        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterLaws();
        }

        private void btnThemLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page21(null, _currentUser));
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder(capacity: normalizedString.Length);

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('�', 'd').Replace('�', 'D').ToLower();
        }

        private void FilterLaws()
        {
            if (lstLuat == null) return;
            string keyword = txtIdentifier.Text ?? "";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            else
            {
                string searchKey = RemoveDiacritics(keyword).Trim();
                var searchWords = searchKey.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var filtered = lstLuat.Where(l => 
                {
                    string combinedText = $"{(l.TenLoi != null ? RemoveDiacritics(l.TenLoi) : "")} " +
                                          $"{(l.PhatTienOto != null ? RemoveDiacritics(l.PhatTienOto) : "")} " +
                                          $"{(l.PhatTienXeMay != null ? RemoveDiacritics(l.PhatTienXeMay) : "")} " +
                                          $"{(l.TruDiem != null ? RemoveDiacritics(l.TruDiem) : "")}";

                    // Kiểm tra Tất cả Các t? kh�a (không ph�n bi?t d?u/hoa thường) c� m?t trong n?i dung luật
                    return searchWords.All(word => combinedText.Contains(word));
                }).ToList();
                dgvDanhSachLuat.ItemsSource = filtered;
            }
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.DataContext is LuatItem selectedLuat)
            {
                NavigationService.Navigate(new Page20(selectedLuat, _currentUser));
            }
        }
    }
}








