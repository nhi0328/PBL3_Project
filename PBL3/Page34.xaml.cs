using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace PBL3
{
    public partial class Page34 : Page
    {
        private readonly Customer _currentUser;
        private readonly Complaint _currentComplaint;

        public Page34() { InitializeComponent(); }
        public Page34(Customer user, Complaint complaint) : this()
        {
            _currentUser = user;
            _currentComplaint = complaint;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;

                myBell.LoadData(_currentUser as Customer);
            }
            this.Loaded += Page34_Loaded;
        }

        private void Page34_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComplaintDetails();
        }

        private void LoadComplaintDetails()
        {
            if (_currentComplaint == null) return;

            tbTitle.Text = _currentComplaint.Title ?? "";
            tbDate.Text = _currentComplaint.SubmitDate.ToString("dd/MM/yyyy");
            tbAddress.Text = _currentComplaint.Address ?? "";
            tbContent.Text = _currentComplaint.Content ?? "";

            if (_currentComplaint.Status == 1)
            {
                tbStatus.Text = "Đã xử lý";
                tbStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            }
            else
            {
                tbStatus.Text = "Chưa xử lý";
                tbStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
            }

            if (!string.IsNullOrEmpty(_currentComplaint.OfficerResponse))
            {
                tbResponse.Text = _currentComplaint.OfficerResponse;
            }
            else
            {
                tbResponse.Text = "Chưa có phản hồi từ cơ quan chức năng.";
            }

            if (!string.IsNullOrEmpty(_currentComplaint.ImagePath) && System.IO.File.Exists(_currentComplaint.ImagePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_currentComplaint.ImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgEvidence.Source = bitmap;
                    tbImageName.Text = System.IO.Path.GetFileName(_currentComplaint.ImagePath);
                }
                catch
                {
                    tbImageName.Text = "Lỗi tải ảnh đính kèm";
                }
            }
            else
            {
                tbImageName.Text = "Không có hình ảnh đính kèm";
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
                NavigationService.Navigate(new Page8(_currentUser));
            }
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
            NavigationService.Navigate(new Page6());
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
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }
    }
}
