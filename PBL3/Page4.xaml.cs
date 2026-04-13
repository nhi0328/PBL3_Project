using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public class ViolationQuickDisplay
    {
        public int STT { get; set; }
        public string Loi { get; set; }
        public string ThoiGian { get; set; }
        public string DiaDiem { get; set; }
        public string TrangThai { get; set; }
    }

    public partial class Page4 : Page
    {
        public Page4()
        {
            InitializeComponent();
        }

        private readonly object _currentUser;

        public Page4(Customer user)
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
            if (sender is Button btn && btn.ContextMenu != null) {            
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page4(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void BtnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chuyển trang Tra cứu luật
        private void BtnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chuyển trang Quản lý phương tiện
        private void BtnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6(_currentUser as Customer));
        }

        //Chuyển trang Quản lý tài khoản
        private void BtnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // chuyển trang Phản ánh
        private void BtnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void txtIdentifier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            if (txtIdentifier == null || dgViolations == null || txtErrorMessage == null) return;

            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                txtErrorMessage.Visibility = Visibility.Collapsed;
                dgViolations.Visibility = Visibility.Collapsed;
                return;
            }

            using var db = new TrafficSafetyDBContext();

            var violations = db.ViolationRecords
                               .Where(r => r.LicensePlate != null && r.LicensePlate.Contains(keyword))
                               .OrderByDescending(r => r.ViolationDate)
                               .ThenByDescending(r => r.ViolationTime)
                               .ToList();

            if (!violations.Any())
            {
                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate.Contains(keyword));

                if (vehicle != null)
                {
                    txtErrorMessage.Text = $"Biển số xe {vehicle.LicensePlate} hiện tại không có lỗi vi phạm nào.";
                }
                else
                {
                    txtErrorMessage.Text = $"Không tìm thấy dữ liệu phương tiện hoặc vi phạm nào cho từ khóa: '{keyword}'.";
                }

                txtErrorMessage.Visibility = Visibility.Visible;
                dgViolations.Visibility = Visibility.Collapsed;
                return;
            }

            txtErrorMessage.Visibility = Visibility.Collapsed;
            dgViolations.Visibility = Visibility.Visible;

            int stt = 1;
            var listSource = violations.Select(v => new ViolationQuickDisplay
            {
                STT = stt++,
                Loi = v.ViolationDescription ?? "Vi phạm giao thông",
                ThoiGian = $"{v.ViolationTime?.ToString(@"hh\:mm")} {v.ViolationDate?.ToString("dd/MM/yyyy")}",
                DiaDiem = v.Address ?? "Không xác định",
                TrangThai = v.StatusText
            }).ToList();

            dgViolations.ItemsSource = listSource;
        }
    }
}
