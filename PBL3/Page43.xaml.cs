using Microsoft.Data.SqlClient;
using PBL3.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PBL3
{
    public partial class Page43 : Page
    {
        private readonly Officer _currentUser;
        private readonly int _violationId;

        // Constructor mặc định
        public Page43()
        {
            InitializeComponent();
        }

        // Constructor test không có Officer
        public Page43(int violationId)
        {
            InitializeComponent();
            _violationId = violationId;
            this.Loaded += Page43_Loaded;
        }

        // Constructor chính
        public Page43(Officer user, int violationId)
        {
            InitializeComponent();
            _currentUser = user;
            _violationId = violationId;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }
            this.Loaded += Page43_Loaded;
        }

        private async void Page43_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();

                var violation = await Task.Run(() => db.ViolationRecords
                    .FirstOrDefault(v => v.ViolationRecordId == _violationId));

                if (violation != null)
                {
                    // Fetch vehicle info
                    var vehicle = await Task.Run(() => db.Vehicles
                         .Include(v => v.VehicleType)
                         .ThenInclude(vt => vt.Category)
                         .FirstOrDefault(v => v.LicensePlate == violation.LicensePlate));

                    txtBienSoHeader.Text = violation.LicensePlate;

                    var descriptionList = violation.ViolationDescription?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) ?? new[] { "Chưa có thông tin" };
                    if (descriptionList.Length > 1) {
                        txtLoiHeader.Text = string.Join("\n", descriptionList.Select((s, index) => $"{index + 1}. {s}"));
                    } else {
                         txtLoiHeader.Text = descriptionList.FirstOrDefault() ?? "Chưa có thông tin";
                    }

                    txtLoaiXe.Text = vehicle?.VehicleType?.Category?.CategoryName ?? "Chưa xác định";
                    txtNgayViPham.Text = violation.ViolationDate?.ToString("dd/MM/yyyy") ?? "Chưa xác định";
                    txtGioViPham.Text = violation.ViolationTime?.ToString(@"hh\:mm") ?? "Chưa xác định";
                    txtDiaDiemViPham.Text = violation.Address ?? "Chưa rõ";
                    txtMoTaViPham.Text = violation.ViolationDescription ?? "Chưa có chi tiết";

                    // We can format the fines/points further if needed based on the table
                    txtMucPhat.Text = "Chưa cập nhật";
                    txtTruDiem.Text = violation.DemeritPoints ?? "0 Điểm";
                    txtDiaDiemDongPhat.Text = "Chưa cập nhật";

                    if (violation.Status == 0) {
                        txtTrangThai.Text = "Chưa xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
                    } else {
                        txtTrangThai.Text = "Đã xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    }

                    if (!string.IsNullOrEmpty(violation.ImagePath))
                    {
                        try {
                            imgViPham.Source = new BitmapImage(new Uri("pack://application:,,," + violation.ImagePath));
                            txtTenAnh.Text = System.IO.Path.GetFileName(violation.ImagePath);
                        } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải chi tiết vi phạm: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            new CustomMessageBox("Chức năng đang phát triển", "Thông báo").ShowDialog();
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
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
                NavigationService.Navigate(_currentUser != null ? new Page24(_currentUser, "unknown") : new Page24("unknown"));
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

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}


