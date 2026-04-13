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
    public partial class Page45 : Page
    {
        private ObservableCollection<LuatItem> lstLuat = new ObservableCollection<LuatItem>();
        private readonly Admin _currentUser;

        // Constructor mặc định
        public Page45()
        {
            InitializeComponent();
            this.Loaded += Page45_Loaded;
        }

        // Constructor chính
        public Page45(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Quản trị viên"; // Hoặc _currentUser.HoTen nếu có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page45_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Dictionary<int, LuatItem> groupedData = new Dictionary<int, LuatItem>();

                using (var db = new TrafficSafetyDBContext())
                {
                    // Lấy Luật kèm theo Chi tiết mức phạt của nó (Quan hệ 1-N)
                    var trafficLaws = db.TrafficLaws
                                        .Include(l => l.Details)
                                            .ThenInclude(d => d.Category)
                                        .ToList();

                    foreach (var law in trafficLaws)
                    {
                        var item = new LuatItem
                        {
                            LawId = law.LawId,
                            TenLoi = law.LawName ?? string.Empty,
                        };

                        // Gom nhóm chi tiết phạt từ bảng TRAFFIC_LAW_DETAILS
                        if (law.Details != null && law.Details.Any())
                        {
                            foreach (var detail in law.Details)
                            {
                                // Lấy căn cứ pháp lý và điểm trừ (ưu tiên lấy cái đầu tiên tìm thấy)
                                if (string.IsNullOrEmpty(item.CanCu)) item.CanCu = detail.Decree ?? string.Empty;
                                if (string.IsNullOrEmpty(item.TruDiem) && detail.DemeritPoints > 0)
                                    item.TruDiem = $"Trừ {detail.DemeritPoints} điểm";

                                // Phân loại mức phạt theo loại xe (Kiểm tra chuỗi)
                                string vehicleType = detail.Category?.CategoryName?.ToLower() ?? "";
                                string fineAmount = detail.FineAmount ?? "";

                                if (vehicleType.Contains("ô tô") || vehicleType.Contains("oto"))
                                {
                                    item.PhatTienOto = fineAmount;
                                }
                                else if (vehicleType.Contains("xe máy") || vehicleType.Contains("mô tô"))
                                {
                                    item.PhatTienXeMay = fineAmount;
                                }
                            }
                        }

                        groupedData[law.LawId] = item;
                    }
                }

                lstLuat = new ObservableCollection<LuatItem>(groupedData.Values);
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi kết nối CSDL: " + ex.Message).ShowDialog();
            }
        }

        // --- CÁC HÀM TÌM KIẾM & LỌC ---
        private void btnSearch_Click(object sender, RoutedEventArgs e) => FilterLaws();
        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e) => FilterLaws();

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D').ToLower();
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

                    return searchWords.All(word => combinedText.Contains(word));
                }).ToList();

                dgvDanhSachLuat.ItemsSource = filtered;
            }
        }

        // --- CÁC NÚT CHỨC NĂNG TRÊN LƯỚI ---
        private void btnThemLuat_Click(object sender, RoutedEventArgs e)
        {
            // Truyền sang trang Thêm Luật
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.DataContext is LuatItem selectedLuat)
            {
                // Truyền LuatItem và _currentUser sang trang Chi tiết 
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


