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
        private readonly Officer _currentUser;

        // Constructor mặc định
        public Page16()
        {
            InitializeComponent();
            LoadData();
        }

        // Constructor nhận thông tin Cán bộ
        public Page16(Officer user) : this()
        {
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }
            LoadData();
        }

        private void LoadData(string searchText = "", string filter = "Tất cả")
        {
            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    // Lấy tất cả phản ánh
                    var query = db.Complaints.AsQueryable();

                    // Tìm kiếm theo từ khóa (Biển số xe, ID người gửi, Tiêu đề hoặc Nội dung)
                    if (!string.IsNullOrEmpty(searchText) && searchText != "Tìm kiếm phản ánh...")
                    {
                        var searchLower = searchText.ToLower();
                        query = query.Where(c =>
                            (c.Content != null && c.Content.ToLower().Contains(searchLower)) ||
                            (c.SenderCitizenId != null && c.SenderCitizenId.ToLower().Contains(searchLower)) ||
                            (c.LicensePlate != null && c.LicensePlate.ToLower().Contains(searchLower)) ||
                            (c.Title != null && c.Title.ToLower().Contains(searchLower))
                        );
                    }

                    // Lọc theo loại (Dựa trên nội dung hoặc Tiêu đề)
                    if (filter == "Tai nạn")
                    {
                        query = query.Where(c => c.Content.Contains("Tai nạn") || c.Title.Contains("Tai nạn"));
                    }
                    else if (filter == "Vi phạm")
                    {
                        query = query.Where(c => c.Content.Contains("Vi phạm") || c.Content.Contains("Lỗi") || c.Title.Contains("Vi phạm"));
                    }

                    var complaintsList = query.OrderByDescending(c => c.SubmitDate).ToList();

                    // Đếm số lượng đơn chưa xử lý (Giả định Status = 0 là chưa xử lý)
                    // (Nếu trong CSDL Nhi để kiểu string "0" thì đổi lại thành c.Status == "0" nhé)
                    int chuaXuLyCount = db.Complaints.Count(c => c.Status == 0);
                    txtChuaXuLy.Text = $"Chưa xử lý ({chuaXuLyCount})";

                    // Map dữ liệu hiển thị lên DataGrid
                    int stt = 1;
                    var displayList = complaintsList.Select(c => new
                    {
                        STT = stt++,
                        ComplaintId = c.ComplaintId,
                        CCCD = c.SenderCitizenId ?? "Không rõ",

                        LoaiPhanAnh = (c.Content != null && c.Content.Contains("Tai nạn")) ? "Tai nạn giao thông" :
                                     ((c.Content != null && (c.Content.Contains("vi phạm") || c.Content.Contains("Lỗi"))) ? "Lỗi vi phạm giao thông" : "Biển báo/Khác"),

                        TieuDe = c.Title ?? "Không có tiêu đề",
                        SubmittedDate = c.SubmitDate != DateTime.MinValue ? c.SubmitDate.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật",

                        // Xử lý hiển thị màu Status
                        Status = (c.Status == 0) ? "Chưa xử lý" : "Đã xử lý",
                        StatusColor = (c.Status == 0) ? "#C62828" : "#2E7D32"
                    }).ToList();

                    dgComplaints.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải dữ liệu phản ánh: " + ex.Message).ShowDialog();
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
                using (var db = new TrafficSafetyDBContext())
                {
                    var list = db.Complaints.Where(c => c.Status == 0)
                                            .OrderByDescending(c => c.SubmitDate)
                                            .ToList();
                    int stt = 1;
                    var displayList = list.Select(c => new
                    {
                        STT = stt++,
                        ComplaintId = c.ComplaintId,
                        CCCD = c.SenderCitizenId ?? "Không rõ",
                        LoaiPhanAnh = (c.Content != null && c.Content.Contains("Tai nạn")) ? "Tai nạn giao thông" :
                                     ((c.Content != null && (c.Content.Contains("vi phạm") || c.Content.Contains("Lỗi"))) ? "Lỗi vi phạm giao thông" : "Biển báo/Khác"),
                        TieuDe = c.Title ?? "Không có tiêu đề",
                        SubmittedDate = c.SubmitDate != DateTime.MinValue ? c.SubmitDate.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật",
                        Status = "Chưa xử lý",
                        StatusColor = "#C62828"
                    }).ToList();

                    dgComplaints.ItemsSource = displayList;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi: " + ex.Message).ShowDialog();
            }
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Lấy ID từ Tag (hoặc CommandParameter) tùy vào XAML của Nhi thiết kế
                int complaintId = 0;

                if (button.Tag != null)
                {
                    int.TryParse(button.Tag.ToString(), out complaintId);
                }
                else if (button.CommandParameter != null)
                {
                    int.TryParse(button.CommandParameter.ToString(), out complaintId);
                }

                // Chuyển trang nếu đã lấy được ID
                if (complaintId != 0)
                {
                    NavigationService.Navigate(new Page26(_currentUser as Officer, complaintId));
                }
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







































