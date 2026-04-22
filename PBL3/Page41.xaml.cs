using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class LicenseItem41
    {
        public string LicenseClass { get; set; }
        public string LicenseNo { get; set; }
        public string Status { get; set; }
        public int Points { get; set; }
        public string IssueDateStr { get; set; }
        public string ExpiryDateStr { get; set; }
        public string IssuePlace { get; set; } = "Ðà N?ng";

        public string StatusText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status)) return "B? thu h?i";
                var s = Status.Trim().ToLower();
                if (s.Contains("ðang ho?t ð?ng") || s.Contains("ho?t ð?ng") || s == "active") return "Ðang ho?t ð?ng";
                if (s.Contains("h?t h?n") || s == "expired") return "H?t h?n";
                return "B? thu h?i";
            }
        }

        public SolidColorBrush StatusColor
        {
            get
            {
                string text = StatusText;
                if (text == "Ðang ho?t ð?ng") return new SolidColorBrush(Color.FromRgb(46, 125, 50));
                if (text == "H?t h?n") return new SolidColorBrush(Color.FromRgb(255, 152, 0));
                return new SolidColorBrush(Color.FromRgb(198, 40, 40));
            }
        }

        public string StatusIcon
        {
            get
            {
                string text = StatusText;
                if (text == "Ðang ho?t ð?ng")
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                if (text == "H?t h?n")
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M11 7H13V13H11V7M11 15H13V17H11V15Z";
                return "M12 2C6.47 2 2 6.47 2 12S6.47 22 12 22 22 17.5 22 12 17.5 2 12 2M17 15.59L15.59 17L12 13.41L8.41 17L7 15.59L10.59 12L7 8.41L8.41 7L12 10.59L15.59 7L17 8.41L13.41 12L17 15.59Z";
            }
        }
    }

    public partial class Page41 : Page
    {
        private readonly Officer _currentUser;
        private readonly string _targetCccd;

        // Constructor m?c ð?nh
        public Page41()
        {
            InitializeComponent();
        }

        // Constructor test không có Officer
        public Page41(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page41_Loaded;
        }

        // Constructor chính
        public Page41(Officer user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán b?: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
            this.Loaded += Page41_Loaded;
        }

        private async void Page41_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            InitViolations();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetCccd)) return;

            try
            {
                using var db = new TrafficSafetyDBContext();

                // L?y thông tin phýõng ti?n
                var vehicle = await Task.Run(() => db.Vehicles
                    .Include(v => v.VehicleType)
                    .ThenInclude(vt => vt.Category)
                    .Include(v => v.VehicleType)
                    .ThenInclude(vt => vt.Color)
                    .Include(v => v.Owner)
                    .FirstOrDefault(v => v.LicensePlate == _targetCccd)); // _targetCccd is actually LicensePlate here

                if (vehicle != null)
                {
                    txtBienSo.Text = vehicle.LicensePlate;
                    txtLoaiXe.Text = vehicle.VehicleType?.Category?.CategoryName ?? "Chýa xác ð?nh";
                    txtNhanHieu.Text = vehicle.VehicleType?.VehicleTypeName ?? "Chýa xác ð?nh";
                    txtNamSanXuat.Text = vehicle.VehicleType?.ManufactureYear?.ToString() ?? "Chýa xác ð?nh";
                    txtMauSac.Text = vehicle.VehicleType?.Color?.ColorName ?? "Chýa xác ð?nh";

                    txtChuSoHuu.Text = vehicle.Owner?.FullName ?? "Chýa xác ð?nh";
                    txtSoKhung.Text = vehicle.ShassisNumber ?? "Chýa xác ð?nh";
                    txtSoMay.Text = vehicle.EngineNumber ?? "Chýa xác ð?nh";
                    txtNgayDangKy.Text = vehicle.RegistrationDate?.ToString("dd/MM/yyyy") ?? "Chýa xác ð?nh";

                    if (vehicle.VehicleType != null && !string.IsNullOrEmpty(vehicle.VehicleType.ImagePath))
                    {
                        try {
                            imgVehicle.Source = new BitmapImage(new Uri("pack://application:,,," + vehicle.VehicleType.ImagePath));
                        } catch { 
                            imgVehicle.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/defaultcar.png"));
                        }
                    }
                    else
                    {
                        imgVehicle.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/defaultcar.png"));
                    }
                }

                // T?i d? li?u vi ph?m
                var violations = await Task.Run(() => db.ViolationRecords
                    .Where(v => v.LicensePlate == _targetCccd)
                    .OrderByDescending(v => v.ViolationDate)
                    .ToList());

                var violationItems = new ObservableCollection<ViolationItem>();
                int stt = 1;
                foreach (var v in violations)
                {
                    string dateStr = v.ViolationDate?.ToString("dd/MM/yyyy") ?? "";
                    string timeStr = v.ViolationTime?.ToString(@"hh\:mm") ?? "";
                    string fullDate = string.IsNullOrEmpty(timeStr) ? dateStr : $"{dateStr}\n{timeStr}";

                    violationItems.Add(new ViolationItem
                    {
                        ViolationId = v.ViolationRecordId,
                        STT = stt++,
                        Date = fullDate,
                        Rule = v.ViolationDescription ?? "Chýa có thông tin",
                        Location = v.Address ?? "Chýa r?",
                        Status = v.Status == 0 ? "Chýa x? l?" : "Ð? x? l?"
                    });
                }

                Violations = violationItems;
                if (FindName("dgViolations") is DataGrid dg)
                {
                    dg.ItemsSource = Violations;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i chi ti?t Phýõng ti?n: " + ex.Message, "L?i k?t n?i").ShowDialog();
            }
        }

        public ObservableCollection<ViolationItem> Violations { get; set; }

        private void InitViolations()
        {
            // Will be initialized in LoadDataAsync
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page42(_currentUser, _targetCccd) : new Page42(_targetCccd));
        }

        private void dgViolations_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgViolations.SelectedItem != null)
            {
                var violation = (ViolationItem)dgViolations.SelectedItem;
                NavigationService.Navigate(_currentUser != null ? new Page43(_currentUser, violation.ViolationId) : new Page43(violation.ViolationId));
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) {
            if (_currentUser is Officer officer)
            {
                new OfficerProfileWindow(officer).ShowDialog();
            }
        }

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

    public class ViolationItem
    {
        public int ViolationId { get; set; }
        public int STT { get; set; }
        public string Date { get; set; }
        public string Rule { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }

        public SolidColorBrush StatusBackground
        {
            get
            {
                if (Status == "Ð? x? l?") return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red
            }
        }

        public string StatusIconPath
        {
            get
            {
                if (Status == "Ð? x? l?") 
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z"; // Check
                return "M13 14H11V9H13M13 18H11V16H13M1 21H23L12 2L1 21Z"; // Alert
            }
        }
    }
}



