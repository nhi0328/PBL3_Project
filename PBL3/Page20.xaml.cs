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
        private readonly Page13LuatItem _currentLuat;
        private readonly Officer _currentUser;

        // Constructor mặc định
        public Page20()
        {
            InitializeComponent();
        }

        public Page20(Page13LuatItem luat, Officer user) : this()
        {
            _currentLuat = luat;
            _currentUser = user;

            // Hiển thị tên cán bộ ở góc phải
            if (_currentUser != null && txtUserName != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
                myBell.LoadData(_currentUser);
            }

            // Gọi hàm tải dữ liệu chi tiết
            LoadLuatDetails();
        }

        private void LoadLuatDetails()
        {
            // Kiểm tra xem có dữ liệu luật truyền sang không
            if (_currentLuat == null) return;

            // Gán tên lỗi lên tiêu đề
            txtTenLoi.Text = _currentLuat.TenLoi;

            string decree = "Chưa có thông tin";
            DateTime? issueDate = null;
            DateTime? effectiveDate = null;
            var detailsList = new List<string>();

            try
            {
                using var db = new TrafficSafetyDBContext();

                // Kéo bảng Categories lên để lấy tên xe (Ô tô, Xe máy...)
                var danhSachCategory = db.Categories.ToList();

                // Lấy dữ liệu gốc từ DB để trích xuất Nghị định và Ngày tháng
                var originalLaw = db.TrafficLaws
                                    .Include(l => l.Details)
                                    .FirstOrDefault(l => l.LawId == _currentLuat.LawId);

                if (originalLaw != null && originalLaw.Details != null)
                {
                    // Lấy thông tin nghị định từ dòng đầu tiên của chi tiết
                    var firstDetail = originalLaw.Details.FirstOrDefault();
                    if (firstDetail != null)
                    {
                        decree = firstDetail.Decree ?? "Chưa có thông tin";
                        issueDate = firstDetail.IssueDate;
                        effectiveDate = firstDetail.EffectiveDate;
                    }

                    // Phân tích từng dòng phạt và điểm trừ
                    foreach (var d in originalLaw.Details)
                    {
                        string catName = "tất cả phương tiện";
                        if (d.CategoryId.HasValue)
                        {
                            var cat = danhSachCategory.FirstOrDefault(c => c.CategoryId == d.CategoryId.Value);
                            if (cat != null) catName = cat.CategoryName.ToLower();
                        }

                        // Thêm dòng Phạt tiền
                        if (!string.IsNullOrEmpty(d.FineAmount))
                        {
                            detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển {catName}");
                        }

                        // Thêm dòng Trừ điểm (Bỏ qua xe đạp ID=3 và loại ID=0)
                        if (d.DemeritPoints.HasValue && d.DemeritPoints.Value > 0 && d.CategoryId != 0 && d.CategoryId != 3)
                        {
                            detailsList.Add($"Trừ {d.DemeritPoints.Value} điểm bằng lái đối với người điều khiển {catName}");
                        }
                    }
                }

                // Quét lịch sử cập nhật lần cuối từ bảng SystemLogs
                var lastLog = db.SystemLogs
                                .Where(log => log.TargetPrefix == "L" && log.TargetValue == _currentLuat.LawId.ToString())
                                .OrderByDescending(log => log.Time)
                                .FirstOrDefault();

                if (txtLastUpdated != null)
                {
                    txtLastUpdated.Text = lastLog != null
                        ? $"Cập nhật lần cuối: {lastLog.Time:HH:mm dd/MM/yyyy}"
                        : "Hệ thống chưa ghi nhận lịch sử chỉnh sửa.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }

            // Đổ dữ liệu lên các TextBlock trên giao diện
            txtNghiDinh.Text = decree;

            if (txtNgayBanHanh != null)
            {
                txtNgayBanHanh.Text = issueDate.HasValue ? $"Ngày ban hành: {issueDate.Value:dd/MM/yyyy}" : "";
                txtNgayBanHanh.Visibility = issueDate.HasValue ? Visibility.Visible : Visibility.Collapsed;
            }

            if (txtNgayHieuLuc != null)
            {
                txtNgayHieuLuc.Text = effectiveDate.HasValue ? $"Ngày có hiệu lực: {effectiveDate.Value:dd/MM/yyyy}" : "";
                txtNgayHieuLuc.Visibility = effectiveDate.HasValue ? Visibility.Visible : Visibility.Collapsed;
            }

            // Đổ danh sách dòng phạt vào ItemsControl
            if (icPunishments != null)
            {
                icPunishments.ItemsSource = detailsList.Distinct().ToList();
            }
        }
        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new OfficerProfileWindow(_currentUser).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiện cửa sổ: " + ex.Message);
            }
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
            if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển sang Page21 (Trang Sửa luật), truyền kèm thông tin Luật và Cán bộ
            NavigationService.Navigate(new Page21(_currentLuat, _currentUser));
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







