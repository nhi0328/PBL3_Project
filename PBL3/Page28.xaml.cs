using PBL3.Models;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PBL3
{
    public class ViolationDetailViewModel
    {
        public int ID { get; set; }
        public int STT { get; set; }
        public string ViolationDateStr { get; set; } = string.Empty;
        public string LawName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
        public string StatusBackground { get; set; } = string.Empty;
    }

    public partial class Page28 : Page
    {
        private readonly string _licensePlate;
        private readonly Customer _currentUser;

        public Page28() { InitializeComponent(); }

        public Page28(Customer user)
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
            NavigationService.Navigate(new Page7(_currentUser as Customer));
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
        public Page28(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        public Page28(string licensePlate, Customer currentUser) : this()
        {
            _licensePlate = licensePlate;
            _currentUser = currentUser;
            this.Loaded += Page28_Loaded;
        }

        private void Page28_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_licensePlate))
            {
                using var db = new TrafficSafetyDBContext();

                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == _licensePlate);
                if (vehicle == null) return;

                var customer = db.Customers.FirstOrDefault(c => c.Cccd == vehicle.Cccd);
                var vType = db.VehicleTypes.FirstOrDefault(vt => vt.VehicleTypeId == vehicle.VehicleTypeId);

                string categoryName = "Không rõ";
                string colorName = "Không rõ";

                if (vType != null)
                {
                    var category = db.Categories.FirstOrDefault(c => c.CategoryId == vType.CategoryId);
                    if (category != null) categoryName = category.CategoryName;

                    var color = db.VehicleColors.FirstOrDefault(c => c.ColorId == vType.ColorId);
                    if (color != null) colorName = color.ColorName;
                }

                // UI bindings
                txtLicensePlate.Text = $"Biển số: {_licensePlate}";
                txtCategory.Text = categoryName;
                txtBrand.Text = vType != null ? vType.VehicleTypeName : "Không rõ";
                txtYear.Text = vType?.ManufactureYear?.ToString() ?? "Không rõ";
                txtColor.Text = colorName;
                if(vType != null && !string.IsNullOrEmpty(vType.ImagePath))
                {
                    // Basic fallback
                }

                txtOwner.Text = customer != null ? customer.FullName : "Không rõ";
                txtChassis.Text = string.IsNullOrEmpty(vehicle.ShassisNumber) ? "Không có" : vehicle.ShassisNumber;
                txtEngine.Text = string.IsNullOrEmpty(vehicle.EngineNumber) ? "Không có" : vehicle.EngineNumber;
                txtRegDate.Text = vehicle.RegistrationDate?.ToString("dd/MM/yyyy") ?? "Không rõ";

                // Load Violations
                var violations = db.ViolationRecords
                    .Where(v => v.LicensePlate == _licensePlate)
                    .ToList();

                var laws = db.TrafficLaws.ToList();

                var violationVMs = new List<ViolationDetailViewModel>();
                int stt = 1;
                foreach (var v in violations)
                {
                    var law = laws.FirstOrDefault(l => l.LawId == v.LawId);
                    string lawName = law != null ? law.LawName : v.ViolationDescription ?? "Vi phạm";
                    string timeStr = v.ViolationTime.HasValue ? v.ViolationTime.Value.ToString(@"hh\:mm") : "00:00";
                    string dateStr = v.ViolationDate.HasValue ? v.ViolationDate.Value.ToString("dd/MM/yyyy") : "01/01/2000";

                    violationVMs.Add(new ViolationDetailViewModel
                    {
                        ID = v.ViolationRecordId,
                        STT = stt++,
                        ViolationDateStr = $"{dateStr}\n{timeStr}",
                        LawName = lawName,
                        Address = v.Address ?? "Không có địa chỉ",
                        StatusText = v.Status == 1 ? "Đã xử lý" : "Chưa xử lý",
                        StatusIcon = v.Status == 1 ? "✔" : "⚠",
                        StatusBackground = v.Status == 1 ? "#388E3C" : "#C62828"
                    });
                }

                dgViolations.ItemsSource = violationVMs;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        private void dgViolations_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgViolations.SelectedItem is ViolationDetailViewModel vm)
            {
                NavigationService.Navigate(new Page29(_currentUser, vm.ID)); // Passing currentUser and ID
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5());
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6());
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8());
        }
    }
}
