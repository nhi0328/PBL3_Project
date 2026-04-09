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

        public Page31(Customer user) : this()
        {
            _currentUser = user;
            this.Loaded += Page31_Loaded;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
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
                new CustomMessageBox("Vui lòng điền đầy đủ thông tin!", "Cảnh báo").ShowDialog();
                return;
            }

            if (newPassword != confirmPassword)
            {
                new CustomMessageBox("Mật khẩu mới và mật khẩu xác nhận không khớp!", "Lỗi").ShowDialog();
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
                        new CustomMessageBox("Sai mật khẩu cũ!", "Lỗi").ShowDialog();
                        return;
                    }

                    customer.Password = newPassword;
                    db.SaveChanges();
                    
                    _currentUser.Password = newPassword;

                    new CustomMessageBox("Đổi mật khẩu thành công!", "Thông báo").ShowDialog();
                    
                    // Clear password boxes
                    pwdOld.Password = "";
                    pwdNew.Password = "";
                    pwdConfirm.Password = "";
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Có lỗi xảy ra: {ex.Message}", "Lỗi").ShowDialog();
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

            // Nếu người dùng chọn "XÁC NHẬN XÓA" (nút đỏ)
            if (confirm.ShowDialog() == true)
            {
                try
                {
                    using var db = new TrafficSafetyDBContext();
                    var customer = db.Customers.FirstOrDefault(c => c.Cccd == _currentUser.Cccd);
                    
                    if (customer != null)
                    {
                        db.Customers.Remove(customer);
                        db.SaveChanges();

                        new CustomMessageBox("Đã xóa tài khoản thành công!").ShowDialog();
                        NavigationService.Navigate(new Page1()); // Quay về trang chủ
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"Xóa tài khoản thất bại: {ex.Message}", "Lỗi").ShowDialog();
                }
            }
            // Nếu chọn "HỦY BỎ" thì không làm gì cả, hộp thoại tự đóng
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
