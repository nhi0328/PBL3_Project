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
    public class LicenseItem40
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
                if (s.Contains("đang hoạt động") || s.Contains("hoạt động") || s == "active") return "Đang hoạt động";
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

    public partial class Page40 : Page
    {
        private readonly Officer _currentUser;
        private readonly string _targetCccd;

        // Constructor mặc định
        public Page40()
        {
            InitializeComponent();
        }

        // Constructor test không có Officer
        public Page40(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page40_Loaded;
        }

        // Constructor chính
        public Page40(Officer user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
            this.Loaded += Page40_Loaded;
        }

        private async void Page40_Loaded(object sender, RoutedEventArgs e)
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

                var dbLicenses = await Task.Run(() => db.Vehicles.Where(l => l.Cccd == _targetCccd).ToList());
                var vehicleList = new List<VehicleViewModel>();

                foreach (var v in dbLicenses)
                {
                    int voCount = await Task.Run(() => db.ViolationRecords.Count(vi => vi.LicensePlate == v.LicensePlate && vi.Status == 0));

                            var vm = new VehicleViewModel
                            {
                                LicensePlate = v.LicensePlate,
                                VehicleName = "Chưa thuộc hãng nào", // We can't easily get Model directly without Include
                                DetailsText = "Xe máy",
                                ImagePath = "/Assets/Images/defaultcar.png",
                                HasViolations = voCount > 0,
                                ViolationCount = voCount
                            };
                            vehicleList.Add(vm);
                        }

                        if (this.FindName("icVehicles") is ItemsControl icVehicles)
                        {
                            icVehicles.ItemsSource = new ObservableCollection<VehicleViewModel>(vehicleList);
                        }
                    }
                    catch (Exception ex)
                    {
                        new CustomMessageBox("Lỗi tải chi tiết PT: " + ex.Message, "Lỗi kết nối").ShowDialog();
                    }
                }

                private string DetermineImagePath(string type, string model)
                {
                    if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(type)) return "/Assets/Images/defaultcar.png";

                    string sModel = model.ToLower();
                    string sType = type.ToLower();

                    if (sType == "xe may")
                    {
                        if (sModel.Contains("sh")) return "/Assets/Images/SH1.png";
                        if (sModel.Contains("evo") || sModel.Contains("vinfast")) return "/Assets/Images/evo1.png";
                        if (sModel.Contains("s1000") || sModel.Contains("s1000rr") || sModel.Contains("bmw")) return "/Assets/Images/s1000rr1.png";
                        return "/Assets/Images/wave1.png";
                    }
                    else // oto
                    {
                        if (sModel.Contains("vf9") || sModel.Contains("vinfast")) return "/Assets/Images/vfe341.png";
                        if (sModel.Contains("civic") || sModel.Contains("honda")) return "/Assets/Images/Hondacivic1.png";
                        if (sModel.Contains("maybach") || sModel.Contains("mercedes")) return "/Assets/Images/Mcmaybach1.png";
                        if (sModel.Contains("aventador") || sModel.Contains("lamborghini")) return "/Assets/Images/Lboghi1.png";
                        return "/Assets/Images/defaultcar.png";
                    }
                }

        private void btnThemGplx_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page42(_currentUser, _targetCccd) : new Page42(_targetCccd));
        }

        private void btnXoaGplx_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VehicleCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is VehicleViewModel vm)
            {
                if (_currentUser != null)
                {
                    NavigationService.Navigate(new Page41(_currentUser, vm.LicensePlate));
                }
                else
                {
                    NavigationService.Navigate(new Page41(vm.LicensePlate));
                }
            }
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


