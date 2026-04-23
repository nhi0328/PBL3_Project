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
    public partial class Page42 : Page
    {
        private readonly Officer _currentUser;
        private readonly string _targetData; // Can be Cccd or LicensePlate
        private bool isEditMode = false;
        private string cccdForAddMode = string.Empty;

        // Constructor m?c đ?nh
        public Page42()
        {
            InitializeComponent();
        }

        // Constructor test không có Officer
        public Page42(string targetData)
        {
            InitializeComponent();
            _targetData = targetData;
            this.Loaded += Page42_Loaded;
        }

        // Constructor chính
        public Page42(Officer user, string targetData)
        {
            InitializeComponent();
            _currentUser = user;
            _targetData = targetData;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán b?: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
            this.Loaded += Page42_Loaded;
        }

        private async void Page42_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadComboboxDataAsync();
            await LoadDataAsync();
        }

        private async Task LoadComboboxDataAsync()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();

                var categories = await Task.Run(() => db.Categories.ToList());
                cbLoaiPhuongTien.ItemsSource = categories;

                var vehicleTypes = await Task.Run(() => db.VehicleTypes.ToList());
                cbNhanHieu.ItemsSource = vehicleTypes;

                var colors = await Task.Run(() => db.VehicleColors.ToList());
                cbMauSac.ItemsSource = colors;
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i chi ti?t: " + ex.Message, "L?i k?t n?i").ShowDialog();
            }
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetData)) return;

            try
            {
                using var db = new TrafficSafetyDBContext();

                // Th? t?m phương ti?n v?i _targetData làm bi?n s?
                var vehicle = await Task.Run(() => db.Vehicles
                    .Include(v => v.VehicleType)
                    .ThenInclude(vt => vt.Category)
                    .Include(v => v.VehicleType)
                    .FirstOrDefault(v => v.LicensePlate == _targetData));

                if (vehicle != null)
                {
                    // Edit mode
                    isEditMode = true;

                    txtBienSo.Text = vehicle.LicensePlate;
                    txtBienSo.IsReadOnly = true; 

                    cbLoaiPhuongTien.SelectedValue = vehicle.VehicleType?.CategoryId;
                    cbNhanHieu.SelectedValue = vehicle.VehicleTypeId;
                    txtNamSanXuat.Text = vehicle.VehicleType?.ManufactureYear?.ToString();
                    cbMauSac.SelectedValue = vehicle.VehicleType?.ColorId;

                    txtSoKhung.Text = vehicle.ShassisNumber;
                    txtSoMay.Text = vehicle.EngineNumber;
                    txtNgayDangKy.Text = vehicle.RegistrationDate?.ToString("dd/MM/yyyy");
                }
                else
                {
                    // Add mode, targetData is likely Cccd
                    isEditMode = false;
                    cccdForAddMode = _targetData;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i chi ti?t: " + ex.Message, "L?i k?t n?i").ShowDialog();
            }
        }

        private void btnThemGplx_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnXoaGplx_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            new CustomMessageBox("Đ? lưu thông tin phương ti?n thành công!", "Thông báo").ShowDialog();
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
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
                NavigationService.Navigate(_currentUser != null ? new Page24(_currentUser, _targetData) : new Page24(_targetData));
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



