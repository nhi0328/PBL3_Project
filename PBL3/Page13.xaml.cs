using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng .Include()
using PBL3.Models;

namespace PBL3
{
    public class Page13LuatItem
    {
        public int LawId { get; set; }
        public string TenLoi { get; set; }
        public List<string> Details { get; set; } // Dùng cho giao diện
        public string ChuoiTimKiem { get; set; } // Dùng cho thuật toán tìm kiếm
        public TrafficLaw OriginalLaw { get; set; }
    }

    public partial class Page13 : Page
    {
        private ObservableCollection<Page13LuatItem> lstLuat = new ObservableCollection<Page13LuatItem>();

        // CHỈ NHẬN OFFICER
        private readonly Officer _currentUser;

        // Constructor mặc định
        public Page13()
        {
            InitializeComponent();
            LoadData();
        }

        // Constructor nhận Officer
        public Page13(Officer user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
                myBell.LoadData(_currentUser as Officer);
            }
        }

        // --- XỬ LÝ MENU DROPDOWN (AVATAR) ---
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new OfficerProfileWindow(_currentUser).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiện cửa sổ: " + ex.Message);
            }
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG BÊN TRÁI ---
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page16(_currentUser));
        private void btnLogOut_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page1());


        private void LoadData()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                var danhSachCategory = db.Categories.ToList();
                var trafficLaws = db.TrafficLaws.Include(l => l.Details).ToList();
                var danhSachGop = new List<Page13LuatItem>();

                foreach (var law in trafficLaws)
                {
                    var detailsList = new List<string>();
                    string searchString = law.LawName ?? "";

                    if (law.Details != null && law.Details.Any())
                    {
                        foreach (var d in law.Details)
                        {
                            string catName = "tất cả phương tiện";
                            if (d.CategoryId.HasValue)
                            {
                                var loaiKhop = danhSachCategory.FirstOrDefault(v => v.CategoryId == d.CategoryId.Value);
                                if (loaiKhop != null) catName = loaiKhop.CategoryName.ToLower();
                            }

                            if (!string.IsNullOrEmpty(d.FineAmount))
                            {
                                if (d.CategoryId == 0)
                                {
                                    detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người {catName}");
                                }
                                else
                                {
                                    detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển {catName}");
                                }
                                searchString += " " + d.FineAmount + " " + catName;
                            }

                            // KIỂM TRA CHẶT: Bỏ qua xe thô sơ và xe đạp (ID 0 và 3)
                            if (d.DemeritPoints.HasValue && d.DemeritPoints.Value > 0 && d.CategoryId != 0 && d.CategoryId != 3)
                            {
                                detailsList.Add($"Trừ {d.DemeritPoints.Value} điểm bằng lái đối với người điều khiển {catName}");
                            }
                        }
                    }

                    detailsList = detailsList.Distinct().ToList();
                    if (detailsList.Count == 0) detailsList.Add("Chưa có thông tin chi tiết mức phạt");

                    danhSachGop.Add(new Page13LuatItem
                    {
                        LawId = law.LawId,
                        TenLoi = law.LawName,
                        Details = detailsList,
                        ChuoiTimKiem = searchString,
                        OriginalLaw = law
                    });
                }

                lstLuat = new ObservableCollection<Page13LuatItem>(danhSachGop);
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
            }
        }

        // --- CÁC HÀM TÌM KIẾM & LỌC ---
        private void btnSearch_Click(object sender, RoutedEventArgs e) => FilterLaws();
        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstLuat == null || lstLuat.Count == 0 || dgvDanhSachLuat == null) return;

            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            else
            {
                string[] searchWords = RemoveDiacritics(keyword).ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var filtered = lstLuat.Where(l =>
                {
                    // Quét trên toàn bộ Chuỗi Tìm Kiếm (đã chứa tên luật + mức phạt + loại xe)
                    string combinedText = RemoveDiacritics(l.ChuoiTimKiem).ToLower();
                    return searchWords.All(word => combinedText.Contains(word));
                }).ToList();

                dgvDanhSachLuat.ItemsSource = filtered;
            }
        }

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
            NavigationService.Navigate(new Page21(null, _currentUser));
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.DataContext is Page13LuatItem selectedLuat)
            {
                // Truyền LuatItem và _currentUser sang trang Chi tiết (Ví dụ Page20)
                var luatItem = new Page13LuatItem 
                {
                    LawId = selectedLuat.LawId,
                    TenLoi = selectedLuat.TenLoi,
                    PhatTienOto = selectedLuat.PhatTienOto,
                    PhatTienXeMay = selectedLuat.PhatTienXeMay,
                    TruDiem = selectedLuat.TruDiem,
                    CanCu = selectedLuat.CanCu,
                    NgayBanHanh = selectedLuat.NgayBanHanh,
                    NgayHieuLuc = selectedLuat.NgayHieuLuc
                };
                // NavigationService.Navigate(new Page20(luatItem, _currentUser));
            }
        }
    }
}