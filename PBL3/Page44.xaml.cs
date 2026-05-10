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
    public partial class Page44 : Page
    {
        private readonly Admin _currentUser;

        // Constructor m?c đ?nh
        public Page44()
        {
            InitializeComponent();
            this.Loaded += Page44_Loaded;
        }

        // Constructor chính
        public Page44(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private async void Page44_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                var categories = db.Categories
                                   .Where(c => c.CategoryName != "Đi bộ" && c.CategoryName != "Xe đạp")
                                   .ToList();
                cboVehicleType.ItemsSource = categories;
                if (categories.Any())
                {
                    cboVehicleType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải loại phương tiện: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void txtIdentifier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            if (txtIdentifier == null || dgViolations == null || txtErrorMessage == null || bdWarning == null || txtWarningMessage == null) return;

            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                txtErrorMessage.Visibility = Visibility.Collapsed;
                bdWarning.Visibility = Visibility.Collapsed;
                dgViolations.Visibility = Visibility.Collapsed;
                return;
            }

            using var db = new TrafficSafetyDBContext();

            // Lấy thông tin Biên bản KÈM THEO thời gian cập nhật mới nhất từ SystemLogs
            var violationsWithLogs = db.ViolationRecords
                .Include(r => r.Law)
                .Where(r => r.LicensePlate != null && r.LicensePlate.Contains(keyword))
                .Select(r => new
                {
                    Record = r,
                    // Tìm Log mới nhất của biên bản này. Nếu không có thì lấy Null
                    LastLogTime = db.SystemLogs
                                    .Where(log => log.TargetPrefix == "B" && log.TargetValue == r.ViolationRecordId.ToString())
                                    .OrderByDescending(log => log.Time)
                                    .Select(log => (DateTime?)log.Time)
                                    .FirstOrDefault()
                })
                .ToList();

            if (!violationsWithLogs.Any())
            {
                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate.Contains(keyword));

                if (vehicle != null)
                {
                    txtErrorMessage.Text = $"Biển số xe {vehicle.LicensePlate} hiện tại không có lỗi vi phạm nào.";
                }
                else
                {
                    txtErrorMessage.Text = $"Không tìm thấy dữ liệu phương tiện hoặc vi phạm nào cho từ khóa: '{keyword}'.";
                }

                txtErrorMessage.Visibility = Visibility.Visible;
                bdWarning.Visibility = Visibility.Collapsed;
                dgViolations.Visibility = Visibility.Collapsed;
                return;
            }

            txtErrorMessage.Visibility = Visibility.Collapsed;

            // Gom nhóm theo Biển số xe. Lưu ý: Lấy từ cái biến Ảo Record vừa tạo ở trên
            var grouped = violationsWithLogs.GroupBy(v => new { v.Record.LicensePlate })
                                            .OrderBy(g => g.All(v => v.Record.Status != 0))
                                            .ToList();

            int stt = 1;
            int totalUnprocessed = 0;
            var listSource = new List<ViolationGroupDisplay>();

            foreach (var group in grouped)
            {
                var first = group.First().Record;
                int groupUnprocessedCount = group.Count(v => v.Record.Status == 0);
                totalUnprocessed += groupUnprocessedCount;

                bool isProcessed = groupUnprocessedCount == 0;

                var listLoi = new List<ViolationDetailDisplay>();
                int loiCount = 1;
                int totalInGroup = group.Count();

                foreach (var v in group.OrderByDescending(x => x.Record.ViolationDate).ThenByDescending(x => x.Record.ViolationTime))
                {
                    string loiName = v.Record.Law?.LawName ?? v.Record.ViolationDescription ?? "Vi phạm giao thông";
                    string prefix = totalInGroup > 1 ? $"{loiCount}. " : "";

                    string timeStr = v.Record.ViolationTime?.ToString(@"hh\:mm") ?? "";
                    string dateStr = v.Record.ViolationDate?.ToString("dd/MM/yyyy") ?? "";
                    string baseTime = $"{dateStr} - {timeStr}";

                    // TÍNH TOÁN THỜI GIAN CẬP NHẬT LẦN CUỐI
                    DateTime thoiGianCuoi = v.LastLogTime ?? v.Record.ViolationDate ?? DateTime.Now;
                    string lastUpdateStr = $"(Cập nhật: {thoiGianCuoi:dd/MM/yy})";

                    // Nối mô tả phụ: Ví dụ: "12/05/2026 - 14:30 (Cập nhật: 13/05/26)"
                    string extraTime = $"{baseTime} {lastUpdateStr}";

                    listLoi.Add(new ViolationDetailDisplay { MoTaLoi = prefix + loiName, ThoiGian = extraTime });
                    loiCount++;
                }

                listSource.Add(new ViolationGroupDisplay
                {
                    STT = stt++,
                    BienSo = first.LicensePlate,
                    DanhSachLoi = listLoi,
                    TrangThaiIcon = isProcessed ? "✓" : "!",
                    TrangThaiText = isProcessed ? "Đã xử lý" : "Chưa xử lý",
                    TrangThaiBg = isProcessed ? "#E8F5E9" : "#C62828",
                    TrangThaiFg = isProcessed ? "#2E7D32" : "White",
                    RecordId = first.ViolationRecordId
                });
            }

            dgViolations.ItemsSource = listSource;
            dgViolations.Visibility = Visibility.Visible;

            if (totalUnprocessed > 0)
            {
                txtWarningMessage.Text = $"Hệ thống ghi nhận có {totalUnprocessed} lỗi vi phạm chưa được xử lý!";
                bdWarning.Visibility = Visibility.Visible;
            }
            else
            {
                bdWarning.Visibility = Visibility.Collapsed;
            }
        }

        // X? l? s? ki?n nút Chi ti?t
        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ViolationGroupDisplay data)
            {
                try
                {
                    // B?T BU?C PH?I NHÉT `data.RecordId` VÀO TRONG NGO?C NHƯ V?Y:
                    NavigationService.Navigate(new Page50(_currentUser, data.RecordId));
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("Lỗi khi chuyển trang: " + ex.Message, "Lỗi").ShowDialog();
                }
            }
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




