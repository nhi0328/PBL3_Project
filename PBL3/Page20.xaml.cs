using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
    public partial class Page20 : Page
    {
        private LuatItem _currentLuat;
        private User _currentUser;

        // Constructor mặc định
        public Page20()
        {
            InitializeComponent();
        }

        public Page20(LuatItem luat, User user = null) : this()
        {
            _currentLuat = luat;
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }

            LoadLuatDetails();
        }

        private void LoadLuatDetails()
        {
            if (_currentLuat == null) return;

            txtTenLoi.Text = _currentLuat.TenLoi;
            txtNghiDinh.Text = _currentLuat.CanCu;
            txtNgayBanHanh.Text = _currentLuat.NgayBanHanh;
            txtNgayHieuLuc.Text = _currentLuat.NgayHieuLuc;

            // Xử lý hiẨn th? m?c ph?t
            if (_currentLuat.HasPhatTienXeMay)
            {
                spPhatXeMay.Visibility = Visibility.Visible;
                txtPhatXeMay.Text = $"Phạt tiền từ {_currentLuat.PhatTienXeMay} đối với người điều khiển xe mô tô, xe máy";
            }
            else
            {
                spPhatXeMay.Visibility = Visibility.Collapsed;
            }

            if (_currentLuat.HasPhatTienOto)
            {
                spPhatOto.Visibility = Visibility.Visible;
                txtPhatOto.Text = $"Phạt tiền từ {_currentLuat.PhatTienOto} đối với người điều khiển xe Ô tô";
            }
            else
            {
                spPhatOto.Visibility = Visibility.Collapsed;
            }

            if (_currentLuat.HasTruDiem)
            {
                spTruDiem.Visibility = Visibility.Visible;
                txtTruDiem.Text = _currentLuat.TruDiem;
            }
            else
            {
                spTruDiem.Visibility = Visibility.Collapsed;
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
        public Page20(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
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
                NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
            }
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat != null)
            {
                NavigationService.Navigate(new Page21(_currentLuat, _currentUser));
            }
        }

        private void btnXoaLuat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat == null) return;

            MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xoá luật '{_currentLuat.TenLoi}' không?", "Xác nhận Xoá", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"
                            DELETE FROM TRAFFIC_LAW_DETAILS WHERE LAW_ID IN (SELECT LAW_ID FROM TRAFFIC_LAWS WHERE LAW_NAME = @TenLoi);
                            DELETE FROM TRAFFIC_LAWS WHERE LAW_NAME = @TenLoi;";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TenLoi", _currentLuat.TenLoi);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Đã xoá luật thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Quay lại trang trước sau khi Xoá
                                if (NavigationService.CanGoBack)
                                {
                                    NavigationService.GoBack();
                                }
                                else
                                {
                                    NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
                                }
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy luật Đã xoá.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi Xoá CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        // Chuyển trang Tra c?u luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        // Chuyển trang Lập biên bản vi phạm
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







