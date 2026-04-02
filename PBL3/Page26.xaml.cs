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

namespace PBL3
{
    public partial class Page26 : Page
    {
        // Biến lưu User đang đăng nhập (Nên truyền từ trang Đăng nhập qua)
        private User _currentUser;
        private int _complaintId;
        private TrafficDbContext _dbContext;

        // Constructor mặc định
        public Page26()
        {
            InitializeComponent();
            _dbContext = new TrafficDbContext();
        }

        public Page26(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _dbContext = new TrafficDbContext();

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        public Page26(User user, int complaintId)
        {
            InitializeComponent();
            _currentUser = user;
            _complaintId = complaintId;
            _dbContext = new TrafficDbContext();

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var complaint = _dbContext.Complaints.FirstOrDefault(c => c.ComplaintId == _complaintId);
                if (complaint != null)
                {
                    txtLoaiPhanAnh.Text = complaint.Title != null && complaint.Title.Contains("Tai nạn") ? "Tai nạn giao thông" : 
                                         (complaint.Title != null && (complaint.Title.Contains("vi phạm") || complaint.Title.Contains("Lỗi")) ? "Vi phạm giao thông" : "Khác");
                    txtNgayPhanAnh.Text = complaint.SubmittedDate.ToString("dd/MM/yyyy");
                    txtTieuDe.Text = complaint.Title;
                    txtNoiDung.Text = complaint.ContentDetail;

                    if (complaint.Status == "0")
                    {
                        txtTrangThai.Text = "Chưa xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                    }
                    else
                    {
                        txtTrangThai.Text = "Đã xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4caf50"));
                    }

                    if (!string.IsNullOrEmpty(complaint.ImagePath))
                    {
                        try
                        {
                            txtImageName.Text = System.IO.Path.GetFileName(complaint.ImagePath);
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(complaint.ImagePath, UriKind.RelativeOrAbsolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            imgPhanAnh.Source = bitmap;
                        }
                        catch
                        {
                            txtImageName.Text = "Không thể tải ảnh";
                        }
                    }
                    else
                    {
                        txtImageName.Text = "Không có hình ảnh";
                    }

                    if (!string.IsNullOrEmpty(complaint.OfficerResponse))
                    {
                        txtPhanHoi.Text = complaint.OfficerResponse;
                    }
                    else
                    {
                        if (complaint.Status == "1" || txtTrangThai.Text == "Đã xử lý")
                        {
                            txtPhanHoi.Text = "Thông qua hình ảnh người dân cung cấp, cơ quan chức năng đã lập biên bản xử phạt phương tiện trên với lỗi tương ứng.";
                            txtPhanHoi.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                        }
                        else
                        {
                            txtPhanHoi.Text = "Nội dung phản hồi";
                            txtPhanHoi.Foreground = new SolidColorBrush(Colors.Gray);
                        }
                    }

                    if (complaint.Status == "0" && (_currentUser is Officer || _currentUser is Admin))
                    {
                        btnXacNhanXuLy.Visibility = Visibility.Visible;
                        txtPhanHoi.IsReadOnly = false;
                    }
                    else
                    {
                        btnXacNhanXuLy.Visibility = Visibility.Collapsed;
                        txtPhanHoi.IsReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết: " + ex.Message);
            }
        }

        private void txtPhanHoi_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!txtPhanHoi.IsReadOnly && txtPhanHoi.Text == "Nội dung phản hồi")
            {
                txtPhanHoi.Text = "";
                txtPhanHoi.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
        }

        private void txtPhanHoi_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!txtPhanHoi.IsReadOnly && string.IsNullOrWhiteSpace(txtPhanHoi.Text))
            {
                txtPhanHoi.Text = "Nội dung phản hồi";
                txtPhanHoi.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void btnXacNhanXuLy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var complaint = _dbContext.Complaints.FirstOrDefault(c => c.ComplaintId == _complaintId);
                if (complaint != null)
                {
                    complaint.Status = "Đã xử lý";
                    string responseText = txtPhanHoi.Text == "Nội dung phản hồi" ? "" : txtPhanHoi.Text;
                    complaint.OfficerResponse = responseText;
                    if (_currentUser is Officer)
                    {
                        complaint.OfficerBadgeNumber = ((Officer)_currentUser).BadgeNumber;
                    }
                    _dbContext.SaveChanges();
                    MessageBox.Show("Đã xác nhận xử lý thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xử lý: " + ex.Message);
            }
        }

        private void LabelQuayLai_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new Page16(_currentUser));
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


















