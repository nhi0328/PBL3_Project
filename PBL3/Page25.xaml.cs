using Microsoft.Data.SqlClient;
using PBL3.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace PBL3
{
    public class LicenseItem
    {
        public string LicenseClass { get; set; }
        public string LicenseNo { get; set; }
        public string Status { get; set; }
        public int Points { get; set; }
        public string IssueDateStr { get; set; }
        public string ExpiryDateStr { get; set; }
        public string IssuePlace { get; set; } = "Đà Nẵng";

        public string StatusText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status)) return "Bị thu hồi";
                var s = Status.Trim().ToLower();
                if (s.Contains("Đang hoạt động") || s.Contains("hoạt động") || s == "active") return "Đang hoạt động";
                if (s.Contains("hết hạn") || s == "expired") return "Hết hạn";
                return "Bị thu hồi";
            }
        }

        public SolidColorBrush StatusColor
        {
            get
            {
                string text = StatusText;
                if (text == "Đang hoạt động") return new SolidColorBrush(Color.FromRgb(46, 125, 50));
                if (text == "Hết hạn") return new SolidColorBrush(Color.FromRgb(255, 152, 0));
                return new SolidColorBrush(Color.FromRgb(198, 40, 40));
            }
        }

        public string StatusIcon
        {
            get
            {
                string text = StatusText;
                if (text == "Đang hoạt động") 
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                if (text == "Hết hạn")
                    return "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M11 7H13V13H11V7M11 15H13V17H11V15Z";
                return "M12 2C6.47 2 2 6.47 2 12S6.47 22 12 22 22 17.5 22 12 17.5 2 12 2M17 15.59L15.59 17L12 13.41L8.41 17L7 15.59L10.59 12L7 8.41L8.41 7L12 10.59L15.59 7L17 8.41L13.41 12L17 15.59Z";
            }
        }
    }

    public partial class Page25 : Page
    {
        private User _currentUser;
        private string _targetCccd;
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True";

        public Page25()
        {
            InitializeComponent();
        }

        public Page25(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page25_Loaded;
        }

        public Page25(User user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
            this.Loaded += Page25_Loaded;
        }

        private async void Page25_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetCccd)) return;

            try
            {
                var licenses = await Task.Run(() =>
                {
                    var list = new List<LicenseItem>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"SELECT LICENSE_CLASS, LICENSE_NUMBER, POINTS, ISSUE_DATE, EXPIRY_DATE, STATUS FROM DRIVING_LICENSES WHERE CITIZEN_ID = @Cccd";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Cccd", _targetCccd);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var item = new LicenseItem
                                    {
                                        LicenseClass = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                        LicenseNo = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                        Points = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                        Status = reader.IsDBNull(5) ? "" : reader.GetString(5)
                                    };

                                    if (!reader.IsDBNull(3))
                                    {
                                        item.IssueDateStr = reader.GetDateTime(3).ToString("dd/MM/yyyy");
                                    }

                                    if (!reader.IsDBNull(4))
                                    {
                                        item.ExpiryDateStr = reader.GetDateTime(4).ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        item.ExpiryDateStr = "Không thểi hẨn";
                                    }

                                    list.Add(item);
                                }
                            }
                        }
                    }
                    return list;
                });

                var icLicenses = this.FindName("icLicenses") as ItemsControl;
                if (icLicenses != null)
                {
                    icLicenses.ItemsSource = new ObservableCollection<LicenseItem>(licenses);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết GPLX: " + ex.Message);
            }
        }

        private void btnThemGplx_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm GPLX đang được cập nhật.");
        }

        private async void btnXoaGplx_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.CommandParameter != null)
            {
                string licenseNo = btn.CommandParameter.ToString();
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xoá GPLX sẽ {licenseNo}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                string query = "DELETE FROM DRIVING_LICENSES WHERE LICENSE_NUMBER = @LicenseNo";
                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    cmd.Parameters.AddWithValue("@LicenseNo", licenseNo);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        });
                        MessageBox.Show("Xoá GPLX thành công!");
                        await LoadDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi Xoá GPLX: " + ex.Message);
                    }
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(_currentUser != null ? new Page24(_currentUser, _targetCccd) : new Page24(_targetCccd));
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


