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
        private readonly LuatItem _currentLuat;
        private readonly Officer _currentUser; // CHỈ NHẬN OFFICER

        // Constructor mặc định
        public Page20()
        {
            InitializeComponent();
        }

        // Constructor chính nhận dữ liệu Luật và Cán bộ
        public Page20(LuatItem luat, Officer user = null) : this()
        {
            _currentLuat = luat;
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
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
            NavigationService.Navigate(new Page24());
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
                    using (var db = new TrafficSafetyDBContext())
                    {
                        // Tìm bộ luật theo LawId (Đảm bảo chính xác 100%)
                        var lawToDelete = db.TrafficLaws.FirstOrDefault(l => l.LawId == _currentLuat.LawId);

                        if (lawToDelete != null)
                        {
                            // Nhờ đã cấu hình Cascade Delete trong Database, 
                            // khi xóa Luật thì các chi tiết (mức phạt Ô tô/Xe máy) trong bảng TRAFFIC_LAW_DETAILS cũng sẽ tự động bay màu theo!
                            db.TrafficLaws.Remove(lawToDelete);
                            db.SaveChanges();

                            new CustomMessageBox("Đã xoá luật thành công.", "Thông báo").ShowDialog();

                            // Trở về trang danh sách luật
                            NavigationService.Navigate(new Page13(_currentUser));
                        }
                        else
                        {
                            new CustomMessageBox("Không tìm thấy luật trên hệ thống. Có thể nó đã bị xóa trước đó.", "Lỗi").ShowDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("Lỗi khi Xoá CSDL: " + ex.Message, "Lỗi").ShowDialog();
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
    }
}







