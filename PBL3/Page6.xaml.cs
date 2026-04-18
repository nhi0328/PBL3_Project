using System.Linq;
using PBL3.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PBL3
{
    public class VehicleViewModel
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string DetailsText { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public bool HasViolations { get; set; }
        public int ViolationCount { get; set; }

        // Formatted properties for View
        public string ViolationText => HasViolations ? $"Có {ViolationCount} vi phạm" : "Không có vi phạm";
        public Visibility ViolationActionVisibility => HasViolations ? Visibility.Visible : Visibility.Collapsed;
        public string IconText => HasViolations ? "⚠" : "✔";
        public string StatusColor => HasViolations ? "Red" : "Green";
    }

    public partial class Page6 : Page
    {
        private readonly Customer _currentUser;

        public Page6() { InitializeComponent(); }

        public Page6(Customer user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = (_currentUser as Customer)?.FullName;
                myBell.LoadData(_currentUser as Customer);
            }
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6()); // Trang thông tin cá nhân
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            // Mở Menu
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page6(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }
        private void Page6_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;

                using var db = new TrafficSafetyDBContext();

                var vehicles = db.Vehicles
                    .Where(v => v.Cccd == _currentUser.Cccd)
                    .Select(v => new
                    {
                        LicensePlate = v.LicensePlate,
                        VehicleName = v.VehicleType != null ? v.VehicleType.VehicleTypeName : "Không xác định",
                        CategoryName = v.VehicleType != null && v.VehicleType.Category != null ? v.VehicleType.Category.CategoryName : "Không rõ loại",
                        ColorName = v.VehicleType != null && v.VehicleType.Color != null ? v.VehicleType.Color.ColorName : "Không rõ màu",
                        ImagePath = v.VehicleType != null && v.VehicleType.ImagePath != null ? v.VehicleType.ImagePath : "/Assets/Images/default_vehicle.png",
                        ViolationCount = v.ViolationRecords.Count(vr => vr.Status == 0) // Chỉ đếm số lỗi chưa xử lý
                    })
                    .ToList();

                var vehicleViewModels = vehicles.Select(v => new VehicleViewModel
                {
                    LicensePlate = v.LicensePlate,
                    VehicleName = v.VehicleName,
                    DetailsText = $"{v.CategoryName} - {v.ColorName}",
                    ImagePath = v.ImagePath,
                    HasViolations = v.ViolationCount > 0,
                    ViolationCount = v.ViolationCount
                }).ToList();

                icVehicles.ItemsSource = vehicleViewModels;
            }
        }

        private void VehicleCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (sender is FrameworkElement element && element.DataContext is VehicleViewModel vm)
                {
                    NavigationService.Navigate(new Page28(vm.LicensePlate, _currentUser));
                }
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6(_currentUser as Customer));
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }
    }
}
