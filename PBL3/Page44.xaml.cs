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

        // Constructor m?c ð?nh
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
                                   .Where(c => c.CategoryName != "Ði b?" && c.CategoryName != "Xe ð?p")
                                   .ToList();
                cboVehicleType.ItemsSource = categories;
                if (categories.Any())
                {
                    cboVehicleType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i lo?i phýõng ti?n: " + ex.Message, "L?i").ShowDialog();
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
                    txtErrorMessage.Text = $"Bi?n s? xe {vehicle.LicensePlate} hi?n t?i không có l?i vi ph?m nào.";
                }
                else
                {
                    txtErrorMessage.Text = $"Không t?m th?y d? li?u phýõng ti?n ho?c vi ph?m nào cho t? khóa: '{keyword}'.";
                }

                txtErrorMessage.Visibility = Visibility.Visible;
                bdWarning.Visibility = Visibility.Collapsed;
                dgViolations.Visibility = Visibility.Collapsed;
                return;
            }

            txtErrorMessage.Visibility = Visibility.Collapsed;

            // Nhóm theo th?i gian, ýu tiên nhóm có l?i chýa x? l? (Status = 0) lên ð?u
            var grouped = violations.GroupBy(v => new { v.LicensePlate, v.ViolationDate, v.ViolationTime })
                                    .OrderBy(g => g.All(v => v.Status != 0)) // Nhóm có ít nh?t 1 l?i Status == 0 s? lên ð?u (v? All return False, False < True)
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
                    string loiName = v.Law?.LawName ?? v.ViolationDescription ?? "Vi ph?m giao thông";
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
                    TrangThaiIcon = isProcessed ? "?" : "?",
                    TrangThaiText = isProcessed ? "Ð? x? l?" : "Chýa x? l?",
                    TrangThaiBg = isProcessed ? "#E8F5E9" : "#C62828",
                    TrangThaiFg = isProcessed ? "#2E7D32" : "White",
                    RecordId = first.ViolationRecordId
                });
            }

            dgViolations.ItemsSource = listSource;
            dgViolations.Visibility = Visibility.Visible;

            if (totalUnprocessed > 0)
            {
                txtWarningMessage.Text = $"H? th?ng ghi nh?n có {totalUnprocessed} l?i vi ph?m chýa ðý?c x? l?!";
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
                    // B?T BU?C PH?I NHÉT `data.RecordId` VÀO TRONG NGO?C NHÝ V?Y:
                    NavigationService.Navigate(new Page50(_currentUser, data.RecordId));
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("L?i khi chuy?n trang: " + ex.Message, "L?i").ShowDialog();
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




