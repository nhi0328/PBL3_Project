using System;
using System.Collections.Generic;
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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{

    public partial class Page12 : Page
    {
        private readonly Officer _currentUser;
        // Constructor mặc định
        public Page12()
        {
            InitializeComponent();
        }

        public Page12(Officer user) : this()
        {
            _currentUser = user;

            // Bảng Officer hiện tại chỉ có OfficerId, CCCD và Password.
            // Để hiển thị, ta sẽ dùng thẳng Mã cán bộ (OfficerId).
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
                myBell.LoadData(_currentUser as Officer);
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
            // Mở Menu
            Button btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page12(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page12());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page13());
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page14());
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page15());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page16());
        }

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        public class ViolationQuickDisplay
        {
            public int RecordId { get; set; }
            public int STT { get; set; }
            public string Loi { get; set; }
            public string ThoiGian { get; set; }
            public string DiaDiem { get; set; }
            public string TrangThai { get; set; }
        }

        private void txtIdentifier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword)) return;

            using var db = new TrafficSafetyDBContext();

            // Lấy thông tin Biên bản KÈM THEO thời gian cập nhật mới nhất từ SystemLogs
            var violationsWithLogs = db.ViolationRecords
                .Include(r => r.Law)
                .Where(r => r.LicensePlate != null && r.LicensePlate.Contains(keyword))
                .Select(r => new
                {
                    Record = r,
                    LastLogTime = db.SystemLogs
                                    .Where(log => log.TargetPrefix == "B" && log.TargetValue == r.ViolationRecordId.ToString())
                                    .OrderByDescending(log => log.Time)
                                    .Select(log => (DateTime?)log.Time)
                                    .FirstOrDefault()
                })
                .OrderByDescending(x => x.Record.ViolationDate)
                .ThenByDescending(x => x.Record.ViolationTime)
                .ToList();

            if (!violationsWithLogs.Any())
            {
                MessageBox.Show($"Không tìm thấy lỗi vi phạm nào cho xe {keyword}", "Thông báo");
                return;
            }

            int stt = 1;
            var listSource = new List<ViolationQuickDisplay>();

            foreach (var v in violationsWithLogs)
            {
                string loiName = v.Record.Law?.LawName ?? v.Record.ViolationDescription ?? "Vi phạm giao thông";
                string timeStr = $"{v.Record.ViolationTime?.ToString(@"hh\:mm")} {v.Record.ViolationDate?.ToString("dd/MM/yyyy")}";

                // Tính toán LastUpdate
                DateTime thoiGianCuoi = v.LastLogTime ?? v.Record.ViolationDate ?? DateTime.Now;
                string lastUpdateStr = $"\n(Cập nhật: {thoiGianCuoi:HH:mm dd/MM})";

                listSource.Add(new ViolationQuickDisplay
                {
                    RecordId = v.Record.ViolationRecordId,
                    STT = stt++,
                    Loi = loiName,
                    ThoiGian = timeStr + lastUpdateStr, // Ghép chữ Cập nhật vào ngay dưới giờ vi phạm
                    DiaDiem = v.Record.Address ?? "Không xác định",
                    TrangThai = v.Record.Status == 1 ? "Đã xử lý" : "Chưa xử lý"
                });
            }

            // Cái DataGrid trong file XAML của Page12 tên là gì thì gọi tên đó ra. Tui giả sử Nhi đặt tên là dgViolations
            // Nếu chưa đặt tên thì mở XAML Page12 lên thêm: x:Name="dgViolations" vào thẻ <DataGrid>
            var dg = FindName("dgViolations") as DataGrid;
            if (dg != null)
            {
                dg.ItemsSource = listSource;
            }
        }

        // Chuyển trang Chi tiết
        private void btnChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var rowData = button?.DataContext as ViolationQuickDisplay;

            if (rowData != null)
            {
                int maBienBan = rowData.RecordId;
                // Chuyển sang Page19, nhớ truyền biến _currentUser và maBienBan qua
                NavigationService.Navigate(new Page19(_currentUser, maBienBan));
            }
        }
    }
}
