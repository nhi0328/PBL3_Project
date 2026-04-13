using Microsoft.Data.SqlClient;
using PBL3.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PBL3
{
    public class LicenseItem
    {
        public string LicenseClass { get; set; }
        public string LicenseNo { get; set; }
        public string Status { get; set; }
        public int Points { get; set; }
        public string IssueDateStr { get; set; }
        public string ExpiryDateStr { get; set; }
        public string IssuePlace { get; set; } = "Đà Nẵng";

        public string StatusText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status)) return "Bị thu hồi";
                var s = Status.Trim().ToLower();
                if (s.Contains("Đang hoạt động") || s.Contains("hoạt động") || s == "active") return "Đang hoạt động";
                if (s.Contains("hết hạn") || s == "expired") return "Hết hạn";
                return "Bị thu hồi";
            }
        }

        public SolidColorBrush StatusColor
        {
            get
            {
                string text = StatusText;
                if (text == "Đang hoạt động") return new SolidColorBrush(Color.FromRgb(46, 125, 50));
                if (text == "Hết hạn") return new SolidColorBrush(Color.FromRgb(255, 152, 0));
                return new SolidColorBrush(Color.FromRgb(198, 40, 40));
            }
        }

        public string StatusIcon
        {
            get
            {
                string text = StatusText;
                if (text == "Đang hoạt động") 
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                if (text == "Hết hạn")
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M11 7H13V13H11V7M11 15H13V17H11V15Z";
                return "M12 2C6.47 2 2 6.47 2 12S6.47 22 12 22 22 17.5 22 12 17.5 2 12 2M17 15.59L15.59 17L12 13.41L8.41 17L7 15.59L10.59 12L7 8.41L8.41 7L12 10.59L15.59 7L17 8.41L13.41 12L17 15.59Z";
            }
        }
    }

    public partial class Page25 : Page
    {
        private readonly Officer _currentUser;
        private readonly string _targetCccd;

        // Constructor mặc định
        public Page25()
        {
            InitializeComponent();
        }

        // Constructor test không có Officer
        public Page25(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page25_Loaded;
        }

        // Constructor chính
        public Page25(Officer user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
            this.Loaded += Page25_Loaded;
        }

        private async void Page25_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetCccd)) return;

            try
            {
                using var db = new TrafficSafetyDBContext();

                // Truy vấn danh sách bằng lái xe của người này
                var dbLicenses = await Task.Run(() => db.DrivingLicenses.Where(l => l.Cccd == _targetCccd).ToList());

                // Map sang danh sách hiển thị
                var licensesList = dbLicenses.Select(l => new LicenseItem
                {
                    LicenseClass = l.LicenseNumber ?? "",
                    LicenseNo = l.LicenseId ?? "",
                    Points = l.Points,
                    Status = l.StatusText ?? "Chưa rõ",
                    IssueDateStr = l.IssueDate != DateTime.MinValue ? l.IssueDate.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                    ExpiryDateStr = l.ExpiryDate.HasValue ? l.ExpiryDate.Value.ToString("dd/MM/yyyy") : "Không thời hạn"
                }).ToList();

                // Đổ vào giao diện
                if (this.FindName("icLicenses") is ItemsControl icLicenses)
                {
                    icLicenses.ItemsSource = new ObservableCollection<LicenseItem>(licensesList);
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải chi tiết GPLX: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }

        private void btnThemGplx_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page39(_currentUser, _targetCccd) : new Page39(_targetCccd));
        }

        private async void btnXoaGplx_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter != null)
            {
                string licenseNo = btn.CommandParameter.ToString();
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xoá vĩnh viễn Giấy phép lái xe số '{licenseNo}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Dùng EF để xóa
                        using var db = new TrafficSafetyDBContext();
                        var licenseToDelete = db.DrivingLicenses.FirstOrDefault(l => l.LicenseId == licenseNo);

                        if (licenseToDelete != null)
                        {
                            db.DrivingLicenses.Remove(licenseToDelete);
                            await db.SaveChangesAsync();
                            new CustomMessageBox("Đã xoá GPLX khỏi hệ thống thành công!", "Thông báo").ShowDialog();

                            // Load lại giao diện
                            await LoadDataAsync();
                        }
                        else
                        {
                            new CustomMessageBox("Không tìm thấy GPLX này để xóa. Có thể nó đã bị xóa trước đó.", "Lỗi").ShowDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        new CustomMessageBox("Lỗi khi Xoá GPLX: " + ex.Message, "Lỗi kết nối").ShowDialog();
                    }
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(_currentUser != null ? new Page24(_currentUser, _targetCccd) : new Page24(_targetCccd));
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
            NavigationService.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }
    }
}


