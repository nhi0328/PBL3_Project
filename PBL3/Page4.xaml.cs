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

        // Đăng xuất
        private void BtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtIdentifier.Text;
            new CustomMessageBox($"Đang tìm kiếm luật với từ khóa: {keyword}").ShowDialog();
            // Viết logic tìm kiếm SQL ở đây...
        }
    }
}
