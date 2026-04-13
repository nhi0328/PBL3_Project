using System;
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
using PBL3.Models;

namespace PBL3
{
    public partial class Page2 : Page
    {
        // Data icon con mắt mở (Mặc định)
        private readonly string iconEyeOpen = "M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z";

        // Data icon con mắt đóng (Có gạch chéo)
        private readonly string iconEyeClosed = "M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.08L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.74,7.13 11.35,7 12,7Z";

        public Page2()
        {
            InitializeComponent();
        }

        // 1. Xử lý khi nhấn nút Mắt
        private void BtnToggleEye_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var iconPath = btn.Template.FindName("IconEye", btn) as Path;

            if (txtPassword.Visibility == Visibility.Visible)
            {
                // Chuyển sang HIỆN mật khẩu
                txtVisiblePassword.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;

                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeClosed);

                txtVisiblePassword.Focus(); // Giữ con trỏ chuột tại ô nhập
            }
            else
            {
                // Chuyển sang ẨN mật khẩu
                txtPassword.Password = txtVisiblePassword.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtVisiblePassword.Visibility = Visibility.Collapsed;

                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeOpen);

                txtPassword.Focus(); // Giữ con trỏ chuột tại ô nhập
            }
        }

        // 2. Xử lý Placeholder cho PasswordBox (Khi gõ ở chế độ ẩn)
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                if (txtPassword.Password.Length > 0) PasswordPlaceholder.Visibility = Visibility.Collapsed;
                else PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        // 3. Xử lý Placeholder cho TextBox (Khi gõ ở chế độ hiện)
        private void TxtVisiblePassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtVisiblePassword.Visibility == Visibility.Visible)
            {
                if (txtVisiblePassword.Text.Length > 0) PasswordPlaceholder.Visibility = Visibility.Collapsed;
                else PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        // 4. CẬP NHẬT NÚT ĐĂNG NHẬP ĐỂ LẤY ĐÚNG BIẾN
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtVisiblePassword.Visibility == Visibility.Visible)
                {
                    txtPassword.Password = txtVisiblePassword.Text;
                }

                string identifier = txtIdentifier.Text.Trim();
                string password = txtPassword.Password;

                object loggedInUser = null;

                // 2. KIỂM TRA 4 TÀI KHOẢN ADMIN ĐẶC BIỆT
                if (identifier.StartsWith("admin") && password == "admin")
                {
                    if (identifier == "admin1" || identifier == "admin2" || identifier == "admin3" || identifier == "admin4")
                    {
                        loggedInUser = new Admin(identifier, "Quản trị viên " + identifier[5..], "admin@traffic.gov.vn", "0123456789", "admin");
                    }
                }

                // 3. KIỂM TRA DATABASE (Nếu không phải admin hệ thống bên trên)
                loggedInUser ??= AuthenticationService.Login(identifier, password);

                // 4. XỬ LÝ KẾT QUẢ
                if (loggedInUser != null)
                {
                    if (loggedInUser is Admin ad)
                    {
                        new CustomMessageBox($"Chào mừng Quản trị viên: {ad.FullName}").ShowDialog();
                        this.NavigationService.Navigate(new Page44());
                    }
                    else if (loggedInUser is Officer off)
                    {
                        new CustomMessageBox($"Chào mừng Cán bộ: {off.OfficerId}").ShowDialog();
                        this.NavigationService.Navigate(new Page12());
                    }
                    else if (loggedInUser is Customer cust)
                    {
                        new CustomMessageBox($"Chào mừng bạn: {cust.FullName}").ShowDialog();
                        this.NavigationService.Navigate(new Page4(cust));
                    }
                }
                else
                {
                    new CustomMessageBox("Tài khoản hoặc mật khẩu không chính xác!", "Lỗi đăng nhập").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi hệ thống: " + ex.Message).ShowDialog();
            }
        }

        private void BtnDangKy_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page3());
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page36());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page1());
        }
    }
}