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
                        TypeId = v.VehicleTypeId,
                        ViolationCount = v.ViolationRecords.Count()
                    })
                    .ToList();

                var vehicleTypes = db.VehicleTypes.ToList();
                var categories = db.Categories.ToList();
                var colors = db.VehicleColors.ToList();

                var vehicleViewModels = new List<VehicleViewModel>();
                foreach(var v in vehicles)
                {
                    var vType = vehicleTypes.FirstOrDefault(vt => vt.VehicleTypeId == v.TypeId);
                    string vehicleName = "Không xác định";
                    string details = "";
                    string img = "/Assets/Images/default_vehicle.png";

                    if (vType != null)
                    {
                        vehicleName = vType.VehicleTypeName;
                        var cat = categories.FirstOrDefault(c => c.CategoryId == vType.CategoryId);
                        var col = colors.FirstOrDefault(c => c.ColorId == vType.ColorId);

                        string catName = cat != null ? cat.CategoryName : "Không rõ loại";
                        string colName = col != null ? col.ColorName : "Không rõ màu";

                        details = $"{catName} - {colName}";

                        if (!string.IsNullOrEmpty(vType.ImagePath))
                        {
                            img = vType.ImagePath;
                        }
                    }

                    vehicleViewModels.Add(new VehicleViewModel
                    {
                        LicensePlate = v.LicensePlate,
                        VehicleName = vehicleName,
                        DetailsText = details,
                        ImagePath = img,
                        HasViolations = v.ViolationCount > 0,
                        ViolationCount = v.ViolationCount
                    });
                }

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
