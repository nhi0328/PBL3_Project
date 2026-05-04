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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public partial class Page47 : Page
    {
        private readonly Admin _currentUser;

        // Constructor m?c đ?nh
        public Page47()
        {
            InitializeComponent();
            this.Loaded += Page47_Loaded;
        }

        // Constructor chính
        public Page47(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page47_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadData();
        }

        private void LoadData(string searchText = "", string filter = "Tất cả")
        {
            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    // L?y t?t c? ph?n ánh
                    var query = db.Complaints.Include(c => c.Vehicle).ThenInclude(v => v.VehicleType).AsQueryable();

                    // T?m ki?m theo t? khóa (Bi?n s? xe, ID ngư?i g?i, Tiêu đ? ho?c N?i dung)
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

                    // L?c theo lo?i
                    if (filter == "Đã xử lý")
                    {
                        query = query.Where(c => c.Status != 0);
                    }
                    else if (filter == "Chưa xử lý")
                    {
                        query = query.Where(c => c.Status == 0);
                    }

                    var complaintsList = query.OrderByDescending(c => c.SubmitDate).ToList();

                    // Đ?m s? lư?ng đơn chưa x? l? (Gi? đ?nh Status = 0 là chưa x? l?)
                    int chuaXuLyCount = db.Complaints.Count(c => c.Status == 0);
                    if (txtChuaXuLy != null)
                    {
                        txtChuaXuLy.Text = $"Chưa xử lý ({chuaXuLyCount})";
                    }

                    // Map d? li?u hi?n th? lên DataGrid
                    int stt = 1;
                    var displayList = complaintsList.Select(c => new
                    {
                        STT = stt++,
                        ComplaintId = c.ComplaintId,
                        LoaiXe = c.Vehicle != null && c.Vehicle.VehicleType != null ? c.Vehicle.VehicleType.VehicleTypeName : "Không xác định",
                        TieuDe = c.Title ?? "Không có tiêu đề",
                        BienSoXe = c.LicensePlate ?? "Không xác định",
                        SubmittedDate = c.SubmitDate != DateTime.MinValue ? c.SubmitDate.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật",
                        Status = (c.Status == 0) ? "Chưa xử lý" : "Đã xử lý",
                        StatusColor = (c.Status == 0) ? "#C62828" : "#2E7D32"
                    }).ToList();

                    if (dgComplaints != null)
                    {
                        dgComplaints.ItemsSource = displayList;
                    }
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

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                if (int.TryParse(btn.Tag.ToString(), out int complaintId))
                {
                    NavigationService.Navigate(new Page54(_currentUser, complaintId));
                }
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item != null)
            {
                var item = row.Item;
                var propertyInfo = item.GetType().GetProperty("ComplaintId");

                if (propertyInfo != null)
                {
                    var complaintIdValue = propertyInfo.GetValue(item, null);

                    if (complaintIdValue != null && int.TryParse(complaintIdValue.ToString(), out int complaintId))
                    {
                        NavigationService.Navigate(new Page54(_currentUser, complaintId));
                    }
                }
            }
        }

        private void btnChuaXuLy_Click(object sender, RoutedEventArgs e)
        {
            // Removed functionality, used to prevent build error during Hot Reload
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { if (_currentUser is Admin admin) { new AdminProfileWindow(admin).ShowDialog(); } }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page44(_currentUser));
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page45(_currentUser));
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page46(_currentUser));
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page47(_currentUser));
        }

        private void btnLichSu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page48(_currentUser));
        }

        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page49(_currentUser));
        }
    }
}




