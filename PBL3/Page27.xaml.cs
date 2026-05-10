using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using PBL3.Models;

namespace PBL3
{
    public partial class Page27 : Page
    {
        private readonly Customer _currentUser;
        private readonly int _lawId; // Biến lưu mã luật

        // Constructor mặc định
        public Page27()
        {
            InitializeComponent();
        }

        // Constructor chính nhận dữ liệu từ Page5 truyền sang
        public Page27(Customer user, int lawId)
        {
            InitializeComponent();
            _currentUser = user;
            _lawId = lawId;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
                myBell.LoadData(_currentUser);
            }

            this.Loaded += Page27_Loaded;
        }

        private void Page27_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLawDetail();
        }

        // Tải chi tiết luật
        private void LoadLawDetail()
        {
            if (_lawId <= 0) return;

            using var db = new TrafficSafetyDBContext();
            var law = db.TrafficLaws.Find(_lawId);
            if (law == null) return;

            txtLawName.Text = law.LawName;

            var danhSachCategory = db.Categories.ToList();
            var details = db.TrafficLawDetails.Where(d => d.LawId == _lawId).ToList();

            string decree = "";
            DateTime? issueDate = null;
            DateTime? effectiveDate = null;
            var detailsList = new List<string>();

            foreach (var d in details)
            {
                // Lấy thông tin ngày tháng
                if (!string.IsNullOrEmpty(d.Decree) && string.IsNullOrEmpty(decree)) decree = d.Decree;
                if (d.IssueDate.HasValue && !issueDate.HasValue) issueDate = d.IssueDate;
                if (d.EffectiveDate.HasValue && !effectiveDate.HasValue) effectiveDate = d.EffectiveDate;

                string catName = "tất cả phương tiện";
                if (d.CategoryId.HasValue)
                {
                    var cat = danhSachCategory.FirstOrDefault(c => c.CategoryId == d.CategoryId.Value);
                    if (cat != null) catName = cat.CategoryName.ToLower();
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
                }

                // Khúc check điều kiện trừ điểm
                if (d.DemeritPoints.HasValue && d.DemeritPoints > 0 && d.CategoryId != 0 && d.CategoryId != 3)
                {
                    detailsList.Add($"Trừ {d.DemeritPoints} điểm bằng lái đối với người điều khiển {catName}");
                }
            }

            txtDecree.Text = !string.IsNullOrEmpty(decree) ? decree : "Chưa có thông tin";

            if (issueDate.HasValue)
            {
                txtIssueDate.Text = $"Ngày ban hành: {issueDate.Value:dd/MM/yyyy}";
                txtIssueDate.Visibility = Visibility.Visible;
            }
            else txtIssueDate.Visibility = Visibility.Collapsed;

            if (effectiveDate.HasValue)
            {
                txtEffectiveDate.Text = $"Ngày có hiệu lực: {effectiveDate.Value:dd/MM/yyyy}";
                txtEffectiveDate.Visibility = Visibility.Visible;
            }
            else txtEffectiveDate.Visibility = Visibility.Collapsed;

            // Đổ vào danh sách phạt
            icPunishments.ItemsSource = detailsList.Distinct().ToList();
        }

        // Nút Quay lại
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        // Các hàm Menu Header của Nhi
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page7(_currentUser));
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page4(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page5(_currentUser));
        private void btnQLPT_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page6(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page7(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page8(_currentUser));
    }
}