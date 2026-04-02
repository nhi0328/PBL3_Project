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
    public partial class Page16 : Page
    {
        // Biến lưu User đang đăng nhập (Nên truyền từ trang Đăng nhập qua)
        private User _currentUser;
        private TrafficDbContext _dbContext;

        // Constructor mặc định
        public Page16()
        {
            InitializeComponent();
            _dbContext = new TrafficDbContext();
            LoadData();
        }

        public Page16(User user)
        {
            InitializeComponent();
            _dbContext = new TrafficDbContext();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
            LoadData();
        }

        // Constructor nhận thông tin User
        public Page16(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void LoadData(string searchText = "", string filter = "Tất cả")
        {
            try
            {
                var query = _dbContext.Complaints.Include(c => c.Ticket).AsQueryable();

                // Lọc theo người dùng nếu là công dân
                if (_currentUser != null && _currentUser is Customer)
                {
                    query = query.Where(c => c.CCCD == _currentUser.Cccd);
                }

                if (!string.IsNullOrEmpty(searchText) && searchText != "Tìm kiếm phản ánh...")
                {
                    query = query.Where(c => c.Content.Contains(searchText) || c.CCCD.Contains(searchText) || c.TicketId.Contains(searchText));
                }

                if (filter == "Tai nạn")
                {
                    query = query.Where(c => c.Content.Contains("Tai nạn"));
                }
                else if (filter == "Vi phạm")
                {
                    query = query.Where(c => c.Content.Contains("Vi phạm") || c.Content.Contains("Lỗi"));
                }

                var complaintsList = query.ToList();

                int chuaXuLyCount = _dbContext.Complaints.Count(c => c.Status == "0");
                if (_currentUser != null && _currentUser is Customer)
                {
                     chuaXuLyCount = _dbContext.Complaints.Count(c => c.CCCD == _currentUser.Cccd && c.Status == "0");
                }
                txtChuaXuLy.Text = $"Chưa xử lý ({chuaXuLyCount})";

                int stt = 1;
                var displayList = complaintsList.Select(c => new
                {
                    STT = stt++,
                    c.ComplaintId,
                    c.CCCD,
                    LoaiPhanAnh = c.Content != null && c.Content.Contains("Tai nạn") ? "Tai nạn giao thông" : 
                                 (c.Content != null && (c.Content.Contains("vi phạm") || c.Content.Contains("Lỗi")) ? "Lỗi vi phạm giao thông" : "Biển báo/Khác"),
                    TieuDe = "Khiếu nại biên bản: " + c.TicketId,
                    SubmittedDate = c.Ticket != null ? c.Ticket.ViolationTime : DateTime.Now,
                    Status = (c.Status == "0") ? "Chưa xử lý" : "Đã xử lý",
                    StatusColor = (c.Status == "0") ? "#C62828" : "#2E7D32"
                }).ToList();

                dgComplaints.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu phản ánh: " + ex.Message);
            }
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Tìm kiếm phản ánh...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Tìm kiếm phản ánh...";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string filter = (cbFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
            LoadData(txtSearch.Text, filter);
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                string filter = (cbFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
                LoadData(txtSearch.Text, filter);
            }
        }

        private void btnChuaXuLy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var query = _dbContext.Complaints.Include(c => c.Ticket).Where(c => c.Status == "0");
                if (_currentUser != null && _currentUser is Customer)
                {
                    query = query.Where(c => c.CCCD == _currentUser.Cccd);
                }

                var list = query.ToList();
                int stt = 1;
                var displayList = list.Select(c => new
                {
                    STT = stt++,
                    c.ComplaintId,
                    c.CCCD,
                    LoaiPhanAnh = c.Content != null && c.Content.Contains("Tai nạn") ? "Tai nạn giao thông" : 
                                 (c.Content != null && (c.Content.Contains("vi phạm") || c.Content.Contains("Lỗi")) ? "Lỗi vi phạm giao thông" : "Biển báo/Khác"),
                    TieuDe = "Khiếu nại biên bản: " + c.TicketId,
                    SubmittedDate = c.Ticket != null ? c.Ticket.ViolationTime : DateTime.Now,
                    Status = "Chưa xử lý",
                    StatusColor = "#C62828"
                }).ToList();

                dgComplaints.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int complaintId)
            {
                NavigationService.Navigate(new Page26(_currentUser, complaintId));
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
                // Công dân: Ẩn các nút chuyển giao diện và thanh kẻ phụ



            }
            else if (_currentUser is Officer)
            {
                // Cán bộ: Được xem giao diện Khách hàng



            }
            else if (_currentUser is Admin)
            {
                // Quản trị viên: Hiện tất cả các lựa chọn để kiểm tra



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

        //Chuyển trang Quản lý tài khoản
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




















