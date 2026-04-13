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
    public partial class Page53 : Page
    {
        private readonly Admin _currentUser;

        // Constructor mặc định
        public Page53()
        {
            InitializeComponent();
            this.Loaded += Page53_Loaded;
        }

        // Constructor chính
        public Page53(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Quản trị viên"; // Hoặc _currentUser.HoTen nếu có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        public Page53(Admin user, int complaintId) : this(user)
        {
            // Do something with complaintId later
        }

        private async void Page53_Loaded(object sender, RoutedEventArgs e)
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

            var violations = db.ViolationRecords.Include(r => r.Law).Where(r => r.LicensePlate != null && r.LicensePlate.Contains(keyword)).ToList();

            if (!violations.Any())
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

            // Nhóm theo thời gian, ưu tiên nhóm có lỗi chưa xử lý (Status = 0) lên đầu
            var grouped = violations.GroupBy(v => new { v.LicensePlate, v.ViolationDate, v.ViolationTime })
                                    .OrderBy(g => g.All(v => v.Status != 0)) // Nhóm có ít nhất 1 lỗi Status == 0 sẽ lên đầu (vì All return False, False < True)
                                    .ThenByDescending(g => g.Key.ViolationDate)
                                    .ThenByDescending(g => g.Key.ViolationTime)
                                    .ToList();

            int stt = 1;
            int totalUnprocessed = 0;
            var listSource = new List<ViolationGroupDisplay>();

            foreach (var group in grouped)
            {
                var first = group.First();
                int groupUnprocessedCount = group.Count(v => v.Status == 0);
                totalUnprocessed += groupUnprocessedCount;

                bool isProcessed = groupUnprocessedCount == 0;

                var listLoi = new List<ViolationDetailDisplay>();
                int loiCount = 1;
                int totalInGroup = group.Count();

                foreach (var v in group)
                {
                    string loiName = v.Law?.LawName ?? v.ViolationDescription ?? "Vi phạm giao thông";
                    string prefix = totalInGroup > 1 ? $"{loiCount}. " : "";

                    string timeStr = v.ViolationTime?.ToString(@"hh\:mm") ?? "";
                    string dateStr = v.ViolationDate?.ToString("dd/MM/yyyy") ?? "";

                    string extraTime = (loiCount == totalInGroup) ? $"{dateStr} - {timeStr}" : "";

                    listLoi.Add(new ViolationDetailDisplay { MoTaLoi = prefix + loiName, ThoiGian = extraTime });
                    loiCount++;
                }

                listSource.Add(new ViolationGroupDisplay
                {
                    STT = stt++,
                    BienSo = first.LicensePlate,
                    DanhSachLoi = listLoi,
                    TrangThaiIcon = isProcessed ? "✔" : "⚠",
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

        // Xử lý sự kiện nút Chi tiết
        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ViolationGroupDisplay data)
            {
                try
                {
                    // BẮT BUỘC PHẢI NHÉT `data.RecordId` VÀO TRONG NGOẶC NHƯ VẦY:
                    NavigationService.Navigate(new Page17(data.RecordId));
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }

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


