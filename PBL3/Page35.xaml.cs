using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;

namespace PBL3
{
    public partial class Page35 : Page
    {
        private readonly Customer _currentUser;
        private string _selectedEvidenceImagePath;

        public Page35() { InitializeComponent(); }
        public Page35(Customer user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;

                myBell.LoadData(_currentUser as Customer);
            }
            this.Loaded += Page35_Loaded;
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

        private void Page35_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using var db = new TrafficSafetyDBContext();
            
            var categories = db.Categories
                               .Where(c => c.CategoryName != "Đi bộ" && c.CategoryName != "Xe đạp")
                               .ToList();
            if (cboVehicleType != null)
            {
                cboVehicleType.ItemsSource = categories;
                cboVehicleType.DisplayMemberPath = "CategoryName";
                cboVehicleType.SelectedValuePath = "CategoryId";
                if (categories.Any()) cboVehicleType.SelectedIndex = 0;
            }

            if (cboProvince != null)
            {
                var provinces = db.Provinces.ToList();
                cboProvince.ItemsSource = provinces;
                cboProvince.DisplayMemberPath = "ProvinceName";
                cboProvince.SelectedValuePath = "ProvinceId";
            }
        }

        private void cboProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProvince.SelectedValue is int provinceId && cboWard != null)
            {
                using var db = new TrafficSafetyDBContext();
                try
                {
                    var property = typeof(TrafficSafetyDBContext).GetProperty("Wards");
                    if (property != null)
                    {
                        dynamic wardsDbSet = property.GetValue(db);
                        var wards = Enumerable.ToList(Enumerable.Where(wardsDbSet, (Func<dynamic, bool>)(w => w.ProvinceId == provinceId)));
                        cboWard.ItemsSource = wards;
                        cboWard.DisplayMemberPath = "WardName";
                        cboWard.SelectedValuePath = "WardId";
                    }
                }
                catch { }
            }
        }

        private void btnConfirmPlate_Click(object sender, RoutedEventArgs e)
        {
            txtLicensePlateStatus.Visibility = Visibility.Collapsed;
            string plate = txtLicensePlate.Text.Trim();
            if (string.IsNullOrEmpty(plate)) return;

            using var db = new TrafficSafetyDBContext();
            bool exists = db.Vehicles.Any(v => v.LicensePlate == plate);
            
            if (!exists)
            {
                txtLicensePlateStatus.Text = $"Biển số xe {plate} chưa được đăng ký.";
                txtLicensePlateStatus.Visibility = Visibility.Visible;
            }
            else
            {
                txtLicensePlateStatus.Text = $"Biển số xe hợp lệ.";
                txtLicensePlateStatus.Foreground = new SolidColorBrush(Colors.Green);
                txtLicensePlateStatus.Visibility = Visibility.Visible;
            }
        }

        private void UploadEvidenceArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Chọn ảnh vi phạm"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedEvidenceImagePath = dialog.FileName;
                txtImagePath.Text = System.IO.Path.GetFileName(_selectedEvidenceImagePath);
                txtImagePath.Visibility = Visibility.Visible;
            }
        }

        private void btnGuiPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = txtTitle.Text.Trim();
                string plate = txtLicensePlate.Text.Trim();
                int? categoryId = cboVehicleType.SelectedValue as int?;
                string addressStr = txtAddress.Text.Trim();
                string content = txtContent.Text.Trim();

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
                {
                    new CustomMessageBox("Vui lòng nhập đầy đủ Tiêu đề và Nội dung!", "Cảnh báo").ShowDialog();
                    return;
                }

                using var db = new TrafficSafetyDBContext();

                int? selectedWardId = null;
                if (cboWard != null && cboWard.SelectedValue != null)
                {
                    selectedWardId = (int)cboWard.SelectedValue;
                }

                var newComplaint = new Complaint
                {
                    SenderCitizenId = _currentUser.Cccd,
                    Title = title,
                    Content = content,
                    LicensePlate = string.IsNullOrEmpty(plate) ? null : plate,
                    CategoryId = categoryId,
                    Address = addressStr,
                    Status = 0,
                    SubmitDate = DateTime.Now
                };

                if (selectedWardId.HasValue)
                {
                    try
                    {
                        var propInfo = typeof(Complaint).GetProperty("WardId");
                        if (propInfo != null) propInfo.SetValue(newComplaint, selectedWardId.Value);
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(_selectedEvidenceImagePath))
                {
                    string destinationDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ComplaintImages");
                    System.IO.Directory.CreateDirectory(destinationDirectory);

                    string extension = System.IO.Path.GetExtension(_selectedEvidenceImagePath);
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
                    string destinationPath = System.IO.Path.Combine(destinationDirectory, fileName);

                    System.IO.File.Copy(_selectedEvidenceImagePath, destinationPath, true);
                    newComplaint.ImagePath = destinationPath;
                }

                db.Complaints.Add(newComplaint);
                db.SaveChanges();

                new CustomMessageBox("Gửi phản ánh thành công!", "Thành công").ShowDialog();
                NavigationService.Navigate(new Page8(_currentUser));
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Có lỗi xảy ra: {ex.Message}", "Lỗi").ShowDialog();
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser));
        }

        //Chẩn qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chẩn trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chẩn trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6());
        }

        //Chẩn trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // Chẩn trang Ph?n ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }
    }
}
