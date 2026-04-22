using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PBL3.Models;

namespace PBL3
{
    public partial class AdminProfileWindow : Window
    {
        public AdminProfileWindow(Admin admin)
        {
            InitializeComponent();

            if (admin != null)
            {
                txtFullName.Text = string.IsNullOrEmpty(admin.FullName) ? "Không xác định" : admin.FullName;
                txtAdminId.Text = string.IsNullOrEmpty(admin.AdminId) ? "Không có ID" : admin.AdminId;
                txtUsername.Text = string.IsNullOrEmpty(admin.Username) ? "Không xác định" : admin.Username;

                // Load Avatar
                bool hasImage = false;
                if (!string.IsNullOrEmpty(admin.ImagePath))
                {
                    try
                    {
                        string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, admin.ImagePath.TrimStart('/'));
                        if (File.Exists(imagePath))
                        {
                            imgAvatar.Source = new BitmapImage(new Uri(imagePath));
                            hasImage = true;
                        }
                    }
                    catch
                    {
                        // Ignore load error
                    }
                }

                if (!hasImage)
                {
                    txtNoImage.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}