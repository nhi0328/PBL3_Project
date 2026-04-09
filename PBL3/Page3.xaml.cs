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
    public partial class Page3 : Page
    {
        // Data Icon Mắt
        private readonly string iconEyeOpen = "M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z";
        private readonly string iconEyeClosed = "M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.08L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.74,7.13 11.35,7 12,7Z";

        public Page3()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Đồng bộ mật khẩu cho CẢ HAI ô nếu đang ở chế độ hiện chữ
                if (txtVisiblePass1.Visibility == Visibility.Visible) Pass1.Password = txtVisiblePass1.Text;
                if (txtVisiblePass2.Visibility == Visibility.Visible) Pass2.Password = txtVisiblePass2.Text;

                string hoTen = $"{txtHo.Text.Trim()} {txtTen.Text.Trim()}";
                string cccd = txtCCCD.Text.Trim();
                string password = Pass1.Password;
                string confirmPass = Pass2.Password;

                // 2. Kiểm tra tính hợp lệ cơ bản
                if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(cccd))
                {
                    new CustomMessageBox("Vui lòng điền đầy đủ Họ tên và CCCD!").ShowDialog();
                    return;
                }

                if (password != confirmPass)
                {
                    new CustomMessageBox("Mật khẩu nhập lại không khớp!").ShowDialog();
                    return;
                }

                DateTime dob = RealDatePicker.SelectedDate ?? new DateTime(2000, 1, 1);
                string gender = (RealComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Khác";

                // 3. Kết nối Database
                using TrafficSafetyDBContext db = new(); 

                // 3.1. Kiểm tra trùng CCCD
                if (db.Customers.Any(u => u.Cccd == cccd))
                {
                    new CustomMessageBox("sẽ CCCD này đã được đăng ký!").ShowDialog();
                    return; // Khi return ở đây, db sẽ tự động được giải phóng (Dispose)
                }

                // 3.2. Đăng ký mới (Đã dùng new() theo IDE0090)
                Customer newUser = new Customer
                {
                    Cccd = cccd,
                    FullName = hoTen,
                    Email = "user@traffic.gov.vn",
                    Phone = "", // Để trống hoặc thêm UI nhập
                    Dob = dob,
                    Gender = gender,
                    Password = password
                };

                db.Customers.Add(newUser);
                db.SaveChanges();

                new CustomMessageBox("Đăng ký thành công!", "Thông báo").ShowDialog();
                NavigationService.Navigate(new Page2());  
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi đăng ký: " + ex.Message).ShowDialog();
            }
        }

        // --- XỬ LÝ MẬT KHẨU 1 ---
        private void BtnTogglePass1_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            var iconPath = btn.Template.FindName("IconEye1", btn) as Path;

            if (Pass1.Visibility == Visibility.Visible)
            {
                txtVisiblePass1.Text = Pass1.Password;
                Pass1.Visibility = Visibility.Collapsed;
                txtVisiblePass1.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeClosed);
            }
            else
            {
                Pass1.Password = txtVisiblePass1.Text;
                txtVisiblePass1.Visibility = Visibility.Collapsed;
                Pass1.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeOpen);
            }
        }

        private void Pass1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Pass1.Visibility == Visibility.Visible)
            {
                if (Pass1.Password.Length > 0) Pass1Place.Visibility = Visibility.Collapsed;
                else Pass1Place.Visibility = Visibility.Visible;
            }
        }

        private void TxtVisiblePass1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtVisiblePass1.Visibility == Visibility.Visible)
            {
                if (txtVisiblePass1.Text.Length > 0) Pass1Place.Visibility = Visibility.Collapsed;
                else Pass1Place.Visibility = Visibility.Visible;
            }
        }

        // --- XỬ LÝ MẬT KHẨU 2 ---
        private void BtnTogglePass2_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            var iconPath = btn.Template.FindName("IconEye2", btn) as Path;

            if (Pass2.Visibility == Visibility.Visible)
            {
                txtVisiblePass2.Text = Pass2.Password;
                Pass2.Visibility = Visibility.Collapsed;
                txtVisiblePass2.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeClosed);
            }
            else
            {
                Pass2.Password = txtVisiblePass2.Text;
                txtVisiblePass2.Visibility = Visibility.Collapsed;
                Pass2.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeOpen);
            }
        }

        private void Pass2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Pass2.Visibility == Visibility.Visible)
            {
                if (Pass2.Password.Length > 0) Pass2Place.Visibility = Visibility.Collapsed;
                else Pass2Place.Visibility = Visibility.Visible;
            }
        }

        private void TxtVisiblePass2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtVisiblePass2.Visibility == Visibility.Visible)
            {
                if (txtVisiblePass2.Text.Length > 0) Pass2Place.Visibility = Visibility.Collapsed;
                else Pass2Place.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page2());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page2());
        }

        private void RealDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePicker.SelectedDate.HasValue)
            {
                DateTime date = RealDatePicker.SelectedDate.Value;
                txtDateDisplay.Text = date.ToString("dd/MM/yyyy");
                txtDateDisplay.Foreground = Brushes.Black;
            }
        }

        private void DateOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            RealDatePicker.Focus();
            RealDatePicker.IsDropDownOpen = true;
        }

        private void GenderOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            RealComboBox.IsDropDownOpen = !RealComboBox.IsDropDownOpen;
        }
    }
}