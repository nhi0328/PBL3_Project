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
        public string Cccd { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = string.Empty;
    }

    public partial class Page15 : Page
    {
        private readonly Officer _currentUser;
        private List<AccountViewModel> _allAccounts = new List<AccountViewModel>();

        // Constructor mặc định
        public Page15()
        {
            InitializeComponent();
            this.Loaded += Page15_Loaded;
        }

        // Constructor nhận thông tin Cán bộ
        public Page15(Officer user) : this()
        {
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
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

                    var customers = db.Customers.ToList();

                    foreach (var c in customers)
                    {
                        accounts.Add(new AccountViewModel
                        {
                            Cccd = c.Cccd ?? "",
                            HoTen = c.FullName ?? "",
                            NgaySinh = c.Dob.HasValue ? c.Dob.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật",
                            GioiTinh = c.Gender ?? "Không rõ"
                        });
                    }
                });
                
                _allAccounts = accounts;
                ApplyFilters();
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải dữ liệu: " + ex.Message).ShowDialog();
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
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
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








