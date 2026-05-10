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
    public partial class Page51 : Page
    {
        private readonly Admin _currentUser;
        private readonly Page45LuatItem _currentLuat;

        // Constructor m?c đ?nh
        public Page51()
        {
            InitializeComponent();
            this.Loaded += Page51_Loaded;
        }

        // Constructor chính
        public Page51(Page45LuatItem luat, Admin user) : this()
        {
            _currentLuat = luat;
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page51_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadData();
        }

        private void LoadData()
        {
            if (_currentLuat == null) return;

            // 1. Đổ dữ liệu cơ bản
            txtTenLoi.Text = _currentLuat.TenLoi;

            string decree = "Chưa có thông tin";
            DateTime? issueDate = null;
            DateTime? effectiveDate = null;
            var detailsList = new List<string>(); // Danh sách chứa các chuỗi hiển thị

            try
            {
                using var db = new TrafficSafetyDBContext();

                // Kéo thêm bảng Categories lên để lấy tên loại xe
                var danhSachCategory = db.Categories.ToList();

                var originalLaw = db.TrafficLaws.Include(l => l.Details).FirstOrDefault(l => l.LawId == _currentLuat.LawId);

                if (originalLaw != null && originalLaw.Details != null)
                {
                    var firstDetail = originalLaw.Details.FirstOrDefault();
                    if (firstDetail != null)
                    {
                        decree = firstDetail.Decree ?? "Chưa có thông tin";
                        issueDate = firstDetail.IssueDate;
                        effectiveDate = firstDetail.EffectiveDate;
                    }

                    // Dùng logic phân tách điểm trừ và loại xe y hệt Page18
                    foreach (var d in originalLaw.Details)
                    {
                        string catName = "tất cả phương tiện";
                        if (d.CategoryId.HasValue)
                        {
                            var cat = danhSachCategory.FirstOrDefault(c => c.CategoryId == d.CategoryId.Value);
                            if (cat != null) catName = cat.CategoryName.ToLower();
                        }

                        if (!string.IsNullOrEmpty(d.FineAmount))
                        {
                            detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển {catName}");
                        }

                        // Kiểm tra điều kiện trừ điểm: Khác 0 và khác 3
                        if (d.DemeritPoints.HasValue && d.CategoryId != 0 && d.CategoryId != 3)
                        {
                            detailsList.Add($"Trừ {d.DemeritPoints.Value} điểm bằng lái đối với người điều khiển {catName}");
                        }
                    }
                }
            }
            catch { }

            txtNghiDinh.Text = decree;

            if (issueDate.HasValue)
            {
                txtNgayBanHanh.Text = $"Ngày ban hành: {issueDate.Value:dd/MM/yyyy}";
                txtNgayBanHanh.Visibility = Visibility.Visible;
            }
            else
            {
                txtNgayBanHanh.Visibility = Visibility.Collapsed;
            }

            if (effectiveDate.HasValue)
            {
                txtNgayHieuLuc.Text = $"Ngày có hiệu lực: {effectiveDate.Value:dd/MM/yyyy}";
                txtNgayHieuLuc.Visibility = Visibility.Visible;
            }
            else
            {
                txtNgayHieuLuc.Visibility = Visibility.Collapsed;
            }

            // 2. Đổ list mức phạt (Dùng danh sách chuỗi vừa phân tích được)
            if (detailsList.Any())
            {
                icPunishments.ItemsSource = detailsList.Distinct().ToList(); // Lọc trùng lặp cho chắc
            }

            // 3. Tìm thời gian cập nhật lần cuối
            try
            {
                using var db = new TrafficSafetyDBContext();
                // Prefix L đại diện cho Law
                var lastLog = db.SystemLogs
                                .Where(log => log.TargetPrefix == "L" && log.TargetValue == _currentLuat.LawId.ToString())
                                .OrderByDescending(log => log.Time)
                                .FirstOrDefault();

                if (lastLog != null)
                {
                    txtLastUpdated.Text = $"Cập nhật lần cuối: {lastLog.Time:HH:mm dd/MM/yyyy}";
                }
                else
                {
                    txtLastUpdated.Text = "Hệ thống chưa ghi nhận lịch sử chỉnh sửa.";
                }
            }
            catch
            {
                txtLastUpdated.Text = "";
            }
        }
        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat != null)
            {
                var l = new Page13LuatItem 
                {
                    LawId = _currentLuat.LawId,
                    TenLoi = _currentLuat.TenLoi
                };
                NavigationService.Navigate(new Page52(l, _currentUser));
            }
        }

        // --- CÁC HÀM T?M KI?M & L?C ---
        private void btnSearch_Click(object sender, RoutedEventArgs e) { }
        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e) { }

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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { if (_currentUser is Admin admin) { new AdminProfileWindow(admin).ShowDialog(); } }

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
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page45(_currentUser));
            }
        }

        private void btnXoaLuat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat == null) return;

            MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xoá luật '{_currentLuat.TenLoi}' không?", "Xác nhận Xoá", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new TrafficSafetyDBContext())
                    {
                        var lawToDelete = db.TrafficLaws.FirstOrDefault(l => l.LawId == _currentLuat.LawId);

                        if (lawToDelete != null)
                        {
                            db.TrafficLaws.Remove(lawToDelete);
                            db.SaveChanges();

                            new CustomMessageBox("Đã xoá luật thành công.", "Thông báo").ShowDialog();

                            NavigationService.Navigate(new Page45(_currentUser));
                        }
                        else
                        {
                            new CustomMessageBox("Không tìm thấy luật trên hệ thống. Có thể nó đã bị xóa trước đó.", "Lỗi").ShowDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("Lỗi khi Xoá CSDL: " + ex.Message, "Lỗi").ShowDialog();
                }
            }
        }
    }
}





