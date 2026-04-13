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
    public partial class Page33 : Page
    {
        private readonly Customer _currentUser;

        public Page33() { InitializeComponent(); }

        public Page33(Customer user) : this()
        {
            _currentUser = user;
            this.Loaded += Page33_Loaded;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;

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
            // M? Menu
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Page33_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                // this.DataContext = new CustomerViewModel(_currentUser);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                if (pb.Name == "pwdNew" && tbNewPwd != null)
                    tbNewPwd.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
                else if (pb.Name == "pwdConfirm" && tbConfirmPwd != null)
                    tbConfirmPwd.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void btnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page7(_currentUser as Customer));
            }
        }

        private void btnLuuMatKhau_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            string newPassword = pwdNew.Password;
            string confirmPassword = pwdConfirm.Password;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                new CustomMessageBox("Vui l?ng ði?n ð?y ð? thông tin!", "C?nh báo").ShowDialog();
                return;
            }

            if (newPassword != confirmPassword)
            {
                new CustomMessageBox("M?t kh?u m?i và m?t kh?u xác nh?n không kh?p!", "L?i").ShowDialog();
                return;
            }

            try
            {
                using var db = new TrafficSafetyDBContext();
                var customer = db.Customers.FirstOrDefault(c => c.Cccd == _currentUser.Cccd);

                if (customer != null)
                {
                    customer.Password = newPassword;
                    db.SaveChanges();

                    _currentUser.Password = newPassword;

                    int targetIdValue = 0;
                    int.TryParse(customer.Cccd, out targetIdValue);

                    var newLog = new SystemLog
                    {
                        Role = 3,                       // 3: Customer
                        Id = customer.Cccd,             // CCCD c?a ngý?i ðang ð?i
                        Action = 2,                     // 2: C?p nh?t
                        TargetPrefix = "C",             // C: Customer
                        TargetValue = targetIdValue.ToString(),
                        Time = DateTime.Now
                    };
                    db.SystemLogs.Add(newLog);
                    db.SaveChanges();

                    // Message box is replaced by visual success text
                    if (tbStatus != null)
                    {
                        tbStatus.Visibility = Visibility.Visible;
                    }

                    // Clear password boxes
                    pwdNew.Password = "";
                    pwdConfirm.Password = "";
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Có l?i x?y ra: {ex.Message}", "L?i").ShowDialog();
            }
        }

        //Chuy?n qua trang Tra c?u nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chuy?n trang Tra c?u lu?t
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chuy?n trang Qu?n l? phýõng ti?n
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6(_currentUser as Customer));
        }

        //Chuy?n trang Qu?n l? tài kho?n
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // chuy?n trang Ph?n ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }

        // Ðãng xu?t
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}
