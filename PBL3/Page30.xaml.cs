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
    public partial class Page30 : Page
    {
        private readonly Customer _currentUser;
        private readonly string _licenseType;

        public Page30() { InitializeComponent(); }

        public Page30(Customer user, string licenseType = null) : this()
        {
            _currentUser = user;
            _licenseType = licenseType;
            this.Loaded += Page30_Loaded;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        private void Page30_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                // TODO: Load violation history for the license type if specified
                // Use _licenseType to filter violations if needed
            }
        }

        private void btnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            // This button handler should not be needed in violation history page
            // If there's a save button in the XAML, its functionality needs to be implemented
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
