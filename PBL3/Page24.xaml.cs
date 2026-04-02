using Microsoft.Data.SqlClient;
using PBL3.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PBL3
{
    public partial class Page24 : Page
    {
        private User _currentUser;
        private string _targetCccd;
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True";

        public Page24()
        {
            InitializeComponent();
        }

        public Page24(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page24_Loaded;
        }

        public Page24(User user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
            this.Loaded += Page24_Loaded;
        }

        private async void Page24_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetCccd)) return;

            try
            {
                string cccd = "";
                string name = "";
                string dob = "";
                string gender = "";
                string phone = "";
                string email = "";
                string imagePath = "";

                await Task.Run(() =>
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"SELECT CCCD, FULL_NAME, DOB, GENDER, PHONE, EMAIL, AVATAR FROM CUSTOMERS WHERE CCCD = @Cccd";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Cccd", _targetCccd);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    cccd = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                    name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                    dob = reader.IsDBNull(2) ? "" : reader.GetDateTime(2).ToString("dd/MM/yyyy");
                                    gender = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                    phone = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                    email = reader.IsDBNull(5) ? "" : reader.GetString(5);
                                    imagePath = reader.IsDBNull(6) ? "" : reader.GetString(6);
                                }
                            }
                        }
                    }
                });

                tbCccd.Text = cccd;
                tbHoTen.Text = name;
                tbNgaySinh.Text = dob;
                tbGioiTinh.Text = gender;
                tbPhone.Text = phone;
                tbEmail.Text = email;

                if (!string.IsNullOrEmpty(imagePath))
                {
                    try
                    {
                        imgAvatar.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi t?i chi ti?t tài khoản: " + ex.Message);
            }
        }

        private void pbNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pbNewPassword.Password))
                tbPasswordPlaceholder.Visibility = Visibility.Visible;
            else
                tbPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private async void btnLuuMatKhau_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = pbNewPassword.Password;
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới.");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "UPDATE CUSTOMERS SET PASSWORD = @Pass WHERE CCCD = @Cccd";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Pass", newPassword);
                            cmd.Parameters.AddWithValue("@Cccd", _targetCccd);
                            cmd.ExecuteNonQuery();
                        }
                    }
                });
                
                MessageBox.Show("cập nhật m?t kh?u thành công!");
                pbNewPassword.Password = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật mật khẩu: " + ex.Message);
            }
        }

        private void btnChiTietGplx_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page25(_currentUser, _targetCccd) : new Page25(_targetCccd));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
            }
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                // Access menu items through the ContextMenu.Items collection




                if (_currentUser is Customer)
                {



                }
                else if (_currentUser is Officer)
                {



                }
                else if (_currentUser is Admin)
                {



                }

                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }
        
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page9());
        }
        
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e) { }
        
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}

