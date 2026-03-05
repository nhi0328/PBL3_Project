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
using PBL3.Models;

namespace PBL3
{
    public partial class Page12 : Page
    {
        // Constructor mặc định
        public Page12()
        {
            InitializeComponent();
        }

        // Biến lưu User đang đăng nhập (Nên truyền từ trang Đăng nhập qua)
        private User _currentUser;

        public Page12(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page());
        }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page9());
        }
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page10());
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            // PHÂN QUYỀN HIỂN THỊ MENU

            if (_currentUser is Customer)
            {
                // Công dân: Ẩn các nút chuyển giao diện và thanh kẻ phụ
                miAdminUI.Visibility = Visibility.Collapsed;
                miOfficerUI.Visibility = Visibility.Collapsed;
                sep1.Visibility = Visibility.Collapsed;
            }
            else if (_currentUser is Officer)
            {
                // Cán bộ: Được xem giao diện Khách hàng
                miAdminUI.Visibility = Visibility.Visible;
                miOfficerUI.Visibility = Visibility.Collapsed;
                sep1.Visibility = Visibility.Visible;
            }
            else if (_currentUser is Admin)
            {
                // Quản trị viên: Hiện tất cả các lựa chọn để kiểm tra
                miAdminUI.Visibility = Visibility.Visible;
                miOfficerUI.Visibility = Visibility.Visible;
                sep1.Visibility = Visibility.Visible;
            }

            // Mở Menu
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page12(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtIdentifier.Text;
            MessageBox.Show($"Đang tìm kiếm luật với từ khóa: {keyword}");
            // Viết logic tìm kiếm SQL ở đây...
        }
    }
}
