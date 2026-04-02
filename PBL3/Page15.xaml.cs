using Microsoft.Data.SqlClient;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

namespace PBL3
{
    public class AccountViewModel
    {
        public int Stt { get; set; }
        public string Cccd { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
    }

    public partial class Page15 : Page
    {
        // Biến lưu User đang đăng nhập (Nên truyền từ trang đăng nhập qua)
        private User _currentUser;
        private List<AccountViewModel> _allAccounts = new List<AccountViewModel>();

        // Constructor mặc định
        public Page15()
        {
            InitializeComponent();
            this.Loaded += Page15_Loaded;
        }

        public Page15(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
            this.Loaded += Page15_Loaded;
        }

        // Constructor nhận thông tin User
        public Page15(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private async void Page15_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var accounts = new List<AccountViewModel>();

                await Task.Run(() =>
                {
                    using TrafficSafetyDBContext db = new TrafficSafetyDBContext();
                    
                    var users = db.Users.ToList();

                    foreach (var u in users)
                    {
                        accounts.Add(new AccountViewModel
                        {
                            Cccd = u.Cccd ?? "",
                            HoTen = u.FullName ?? "",
                            NgaySinh = u.Dob.HasValue ? u.Dob.Value.ToString("dd/MM/yyyy") : "",
                            GioiTinh = u.Gender ?? ""
                        });
                    }
                });
                
                _allAccounts = accounts;
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void ApplyFilters()
        {
            if (_allAccounts == null || dgAccounts == null) return;

            string keyword = txtSearch?.Text?.Trim().ToLower() ?? "";
            
            var filtered = _allAccounts.AsEnumerable();

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(a => 
                    a.Cccd.ToLower().Contains(keyword) || 
                    a.HoTen.ToLower().Contains(keyword));
            }

            var finalResult = filtered.ToList();
            for (int i = 0; i < finalResult.Count; i++)
            {
                finalResult[i].Stt = i + 1;
            }

            dgAccounts.ItemsSource = finalResult;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string cccd)
            {
                NavigationService?.Navigate(_currentUser != null ? new Page24(_currentUser, cccd) : new Page24(cccd));
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page());
        }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page9());
        }
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page10());
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            // PHÂN QUYỀN HIỂN THỊ MENU

            if (_currentUser is Customer)
            {
                // Công dân: Ẩn Các n�t chuyẨn giao diện v� thanh kẻ phụ



            }
            else if (_currentUser is Officer)
            {
                // Cán bộ: Được xem giao diện Khách hàng



            }
            else if (_currentUser is Admin)
            {
                // Quản trị viên: HiẨn Tất cả Các lựa chọn để ki?m tra



            }

            // Mở Menu
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        // Chuyển trang LBBVP
        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        //Chuyển trang QuẨn l? tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}








