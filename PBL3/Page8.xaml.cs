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
    public partial class Page8 : Page
    {
        private readonly Customer _currentUser;

        public Page8() { InitializeComponent(); }

        public Page8(Customer user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = (_currentUser as Customer)?.FullName;
                myBell.LoadData(_currentUser as Customer);
            }
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

        // Constructor nhận thông tin User
        public Page8(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Page8_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComplaints();
        }

        private void LoadComplaints()
        {
            if (_currentUser == null) return;

            string keyword = txtSearch?.Text?.Trim() ?? "";

            using var db = new TrafficSafetyDBContext();
            
            var query = db.Complaints.Include(c => c.Category)
                                     .Where(c => c.SenderCitizenId == _currentUser.Cccd)
                                     .ToList();

            int filterIndex = cbFilter?.SelectedIndex ?? 0;
            switch (filterIndex)
            {
                case 1: // Đã xử lý
                    query = query.Where(c => c.Status == 1).ToList();
                    break;
                case 2: // Chưa xử lý
                    query = query.Where(c => c.Status == 0).ToList();
                    break;
            }

            var scoredList = query.Select(c => new
            {
                Complaint = c,
                Score = string.IsNullOrEmpty(keyword) ? 100 : GetComplaintScore(c, keyword)
            }).Where(x => x.Score > 0).ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                scoredList = scoredList.OrderByDescending(x => x.Score).ToList();
            }
            else
            {
                switch (filterIndex)
                {
                    case 3: // Mới nhất
                        scoredList = scoredList.OrderByDescending(x => x.Complaint.SubmitDate).ToList();
                        break;
                    case 4: // Cũ nhất
                        scoredList = scoredList.OrderBy(x => x.Complaint.SubmitDate).ToList();
                        break;
                    case 5: // A-Z
                        scoredList = scoredList.OrderBy(x => x.Complaint.Title).ToList();
                        break;
                    case 6: // Z-A
                        scoredList = scoredList.OrderByDescending(x => x.Complaint.Title).ToList();
                        break;
                    default:
                        scoredList = scoredList.OrderByDescending(x => x.Complaint.SubmitDate).ToList();
                        break;
                }
            }

            if (dgComplaints != null)
            {
                dgComplaints.ItemsSource = scoredList.Select(x => x.Complaint).ToList();
            }
        }

        private int GetComplaintScore(Complaint c, string keyword)
        {
            int scoreTitle = SearchEngine.CalculateScore(c.Title ?? "", keyword);
            int scoreContent = SearchEngine.CalculateScore(c.Content ?? "", keyword);
            int scoreType = SearchEngine.CalculateScore(c.Category?.CategoryName ?? "", keyword);
            int scorePlate = SearchEngine.CalculateScore(c.LicensePlate ?? "", keyword);
            
            return new[] { scoreTitle, scoreContent, scoreType, scorePlate }.Max();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadComplaints();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadComplaints();
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadComplaints();
        }

        private void dgComplaints_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgComplaints.SelectedItem is Complaint selected)
            {
                NavigationService.Navigate(new Page34(_currentUser, selected));
            }
        }

        private void btnGuiPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page35(_currentUser));
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
            NavigationService.Navigate(new Page6(   ));
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

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }
    }
}
