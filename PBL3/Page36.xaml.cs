using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class Page36 : Page
    {
        public Page36()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtError.Visibility = Visibility.Collapsed;
                string identifier = txtIdentifier.Text.Trim();

                if (string.IsNullOrEmpty(identifier))
                {
                    txtError.Text = "Vui lòng nhập số CCCD";
                    txtError.Visibility = Visibility.Visible;
                    return;
                }

                using TrafficSafetyDBContext db = new TrafficSafetyDBContext();
                var customer = db.Customers.FirstOrDefault(u => u.Cccd == identifier);

                if (customer != null)
                {
                    NavigationService.Navigate(new Page37(customer.Cccd));
                }
                else
                {
                    txtError.Text = "Số CCCD này chưa được đăng ký";
                    txtError.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi hệ thống: " + ex.Message).ShowDialog();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page1());
        }
    }
}