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
    public partial class Page31 : Page
    {
        private readonly Customer _currentUser;

        public Page31() { InitializeComponent(); }

        public Page31(Customer user)
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
            // M? Menu
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nh?n thông tin User
        public Page31(string tenNguoiDung) : this()
        {
            // Ki?m tra n?u có tên th? gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Page31_Loaded(object sender, RoutedEventArgs e)
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
                if (pb.Name == "pwdOld" && tbOldPwd != null)
                    tbOldPwd.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
                else if (pb.Name == "pwdNew" && tbNewPwd != null)
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

            string oldPassword = pwdOld.Password;
            string newPassword = pwdNew.Password;
            string confirmPassword = pwdConfirm.Password;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
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
                    if (customer.Password != oldPassword)
                    {
                        new CustomMessageBox("Sai m?t kh?u c?!", "L?i").ShowDialog();
                        return;
                    }

                    customer.Password = newPassword;
                    db.SaveChanges();

                    int targetIdValue = 0;
                    int.TryParse(customer.Cccd, out targetIdValue);

                    var newLog = new SystemLog
                    {
                        Role = 3,                       // 3: Customer
                        Id = customer.Cccd,             // CCCD c?a ngý?i th?c hi?n
                        Action = 2,                     // 2: C?p nh?t
                        TargetPrefix = "C",             // C: Customer
                        TargetValue = targetIdValue.ToString(),
                        Time = DateTime.Now
                    };
                    db.SystemLogs.Add(newLog);
                    db.SaveChanges();

                    _currentUser.Password = newPassword;

                    new CustomMessageBox("Ð?i m?t kh?u thành công!", "Thông báo").ShowDialog();
                    
                    // Clear password boxes
                    pwdOld.Password = "";
                    pwdNew.Password = "";
                    pwdConfirm.Password = "";
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Có l?i x?y ra: {ex.Message}", "L?i").ShowDialog();
            }
        }

        private void btnQuenMatKhau_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page32(_currentUser as Customer));
        }

        private void btnXoaTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            ConfirmDeleteBox confirm = new ConfirmDeleteBox();

            // N?u ngý?i dùng ch?n "XÁC NH?N XÓA" (nút ð?)
            if (confirm.ShowDialog() == true)
            {
                try
                {
                    using var db = new TrafficSafetyDBContext();
                    var customer = db.Customers.FirstOrDefault(c => c.Cccd == _currentUser.Cccd);
                    
                    if (customer != null)
                    {
                        int targetIdValue = 0;
                        int.TryParse(customer.Cccd, out targetIdValue);

                        var newLog = new SystemLog
                        {
                            Role = 3,                       // 3: Customer
                            Id = customer.Cccd,
                            Action = 3,                     // 3: Xóa
                            TargetPrefix = "C",             // C: Customer
                            TargetValue = targetIdValue.ToString(),
                            Time = DateTime.Now
                        };
                        db.SystemLogs.Add(newLog);
                        db.Customers.Remove(customer);
                        db.SaveChanges();
                        new CustomMessageBox("Ð? xóa tài kho?n thành công!").ShowDialog();
                        NavigationService.Navigate(new Page1()); // Quay v? trang ch?
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Xóa tài kho?n th?t b?i: {ex.Message}", "L?i").ShowDialog();
                }
            }
            // N?u ch?n "H?Y B?" th? không làm g? c?, h?p tho?i t? ðóng
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

    }
}
