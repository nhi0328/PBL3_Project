using PBL3.Models;
using PBL3.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBL3
{
    public partial class Page7 : Page
    {
        private readonly Customer _currentUser;

        public Page7() { InitializeComponent(); }

        public Page7(Customer user)
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
        public Page7(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Page7_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                this.DataContext = new CustomerViewModel(_currentUser);

                using var db = new TrafficSafetyDBContext();
                var licenses = db.DrivingLicenses.Where(l => l.Cccd == _currentUser.Cccd).ToList();

                var vms = new List<LicenseViewModel>();
                foreach (var l in licenses)
                {
                    bool active = l.Status == 1; // Adjust based on DB structure

                    vms.Add(new LicenseViewModel
                    {
                        LicenseType = l.LicenseId ?? "Không rõ",
                        StatusBackground = "#FCEB9C", // yellow color as per the screenshot
                        StatusColor = active ? "Green" : "Red",
                        StatusIcon = active ? "✔" : "⚠",
                        StatusText = active ? "Đang hoạt động" : "Bị thu hồi",
                        PointsText = $"Điểm còn lại của GPLX: {l.Points}",
                        LastUpdateText = "Cập nhật lần cuối: " + DateTime.Now.ToString("HH:mm dd/MM/yyyy"), // Replace with actual trigger info if needed
                        LicenseNumber = l.LicenseNumber,
                        DemeritPoints = l.DemeritPoints,
                        ExpiryDateText = l.ExpiryDate.HasValue ? l.ExpiryDate.Value.ToString("dd/MM/yyyy") : "Không thời hạn",
                        IssueDateText = l.IssueDate.ToString("dd - MM - yyyy"),
                        PlaceOfIssue = "Đà Nẵng" // If WARD_ID or similar exist in DrivingLicense can map it
                    });
                }

                icLicenses.ItemsSource = vms;
            }
        }

        private void btnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CustomerViewModel vm && _currentUser != null)
            {
                try
                {
                    using var db = new TrafficSafetyDBContext();
                    var customer = db.Customers.FirstOrDefault(c => c.Cccd == _currentUser.Cccd);

                    if (customer != null)
                    {
                        customer.FullName = $"{vm.LastName} {vm.FirstName}".Trim();

                        if (int.TryParse(vm.BirthYear, out int year) &&
                            int.TryParse(vm.BirthMonth, out int month) &&
                            int.TryParse(vm.BirthDay, out int day))
                        {
                            try
                            {
                                customer.Dob = new DateTime(year, month, day);
                            }
                            catch
                            {
                                new CustomMessageBox("Ngày sinh không hợp lệ.", "Lỗi").ShowDialog();
                                return;
                            }
                        }

                        if (vm.IsMale) customer.Gender = "Nam";
                        else if (vm.IsFemale) customer.Gender = "Nữ";
                        else customer.Gender = "Khác";

                        customer.Phone = vm.PhoneNumber;
                        customer.Email = vm.Email;

                        // CCCD is typically a primary key and isn't updated here

                        db.SaveChanges();

                        // Update current user
                        _currentUser.FullName = customer.FullName;
                        _currentUser.Dob = customer.Dob;
                        _currentUser.Gender = customer.Gender;
                        _currentUser.Phone = customer.Phone;
                        _currentUser.Email = customer.Email;

                        txtUserName.Text = _currentUser.FullName;
                        new CustomMessageBox("Cập nhật thông tin thành công!", "Thông báo").ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Có lỗi xảy ra: {ex.Message}", "Lỗi").ShowDialog();
                }
            }
        }

        private void btnLichSuTruDiem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                using var db = new TrafficSafetyDBContext();

                // 1. Ánh xạ CCCD qua bảng VEHICLES để tìm các biển số xe (LICENSE_PLATE)
                var vehicles = db.Vehicles.Where(v => v.Cccd == _currentUser.Cccd).Select(v => v.LicensePlate).ToList();

                // 2. Tiếp tục ánh xạ qua bảng VIOLATION_RECORDS để kiểm tra lịch sử trừ điểm
                var hasViolations = db.ViolationRecords.Any(v => vehicles.Contains(v.LicensePlate) && v.DemeritPoints != null && v.DemeritPoints != "0");

                if (!hasViolations)
                {
                    new CustomMessageBox("Hiện tại không có lịch sử vi phạm trừ điểm phạt nguội nào cho các phương tiện của bạn.", "Thông báo").ShowDialog();
                }
                else
                {
                    NavigationService.Navigate(new Page30(_currentUser));
                }
            }
        }

        private void btnBaoMat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page31(_currentUser));
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

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}
