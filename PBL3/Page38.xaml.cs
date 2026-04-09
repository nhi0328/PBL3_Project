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
    public partial class Page38 : Page
    {
        private readonly string iconEyeOpen = "M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z";
        private readonly string iconEyeClosed = "M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.08L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.74,7.13 11.35,7 12,7Z";

        public Page38()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page1());
        }

        private void BtnSavePassword_Click(object sender, RoutedEventArgs e)
        {
            // Add password update logic here later
            NavigationService.Navigate(new Page1());
        }

        private void BtnToggleEye_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var iconPath = btn.Template.FindName("IconEye", btn) as Path;

            if (txtNewPassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtNewPassword.Password;
                txtNewPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeClosed);
                txtVisiblePassword.Focus();
            }
            else
            {
                txtNewPassword.Password = txtVisiblePassword.Text;
                txtNewPassword.Visibility = Visibility.Visible;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeOpen);
                txtNewPassword.Focus();
            }
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtNewPassword.Visibility == Visibility.Visible)
            {
                if (txtNewPassword.Password.Length > 0) PasswordPlaceholder.Visibility = Visibility.Collapsed;
                else PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void TxtVisiblePassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtVisiblePassword.Visibility == Visibility.Visible)
            {
                if (txtVisiblePassword.Text.Length > 0) PasswordPlaceholder.Visibility = Visibility.Collapsed;
                else PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void BtnToggleConfirmEye_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var iconPath = btn.Template.FindName("IconConfirmEye", btn) as Path;

            if (txtConfirmPassword.Visibility == Visibility.Visible)
            {
                txtVisibleConfirmPassword.Text = txtConfirmPassword.Password;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
                txtVisibleConfirmPassword.Visibility = Visibility.Visible;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeClosed);
                txtVisibleConfirmPassword.Focus();
            }
            else
            {
                txtConfirmPassword.Password = txtVisibleConfirmPassword.Text;
                txtConfirmPassword.Visibility = Visibility.Visible;
                txtVisibleConfirmPassword.Visibility = Visibility.Collapsed;
                if (iconPath != null) iconPath.Data = Geometry.Parse(iconEyeOpen);
                txtConfirmPassword.Focus();
            }
        }

        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtConfirmPassword.Visibility == Visibility.Visible)
            {
                if (txtConfirmPassword.Password.Length > 0) ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
                else ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void TxtVisibleConfirmPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtVisibleConfirmPassword.Visibility == Visibility.Visible)
            {
                if (txtVisibleConfirmPassword.Text.Length > 0) ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
                else ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }
    }
}