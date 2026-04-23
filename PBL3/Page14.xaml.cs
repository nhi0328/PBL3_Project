using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PBL3
{
    public class ViolationViewModel
    {
        public int Stt { get; set; }
        public int RecordId { get; set; }
        public string BienSoXe { get; set; } = string.Empty;
        public string HangXe { get; set; } = string.Empty; // Nếu có dữ liệu Hãng xe, có thể join với bảng Vehicles
        public string LoiViPham { get; set; } = string.Empty;
        public string ThoiGian { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public DateTime NgayViPham { get; set; }
    }

    public partial class Page14 : Page
    {
        private readonly Officer _currentUser;
        private List<ViolationViewModel> _allViolations = new List<ViolationViewModel>();
        private bool _isNavigating = false;
        private bool _isDisposed = false;
        private CancellationTokenSource _cts;

        // Constructor mặc định
        public Page14()
        {
            InitializeComponent();
            this.Loaded += Page14_Loaded;
            this.Unloaded += Page14_Unloaded;
        }

        public Page14(Officer user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
        }

        // --- XỬ LÝ VÒNG ĐỜI CỦA PAGE ---
        private async void Page14_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            _isNavigating = false;
            _isDisposed = false;

            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch (ObjectDisposedException) { }

            _cts = new CancellationTokenSource();

            try
            {
                await LoadDataAsync();
            }
            catch (OperationCanceledException) { }
        }

        private void Page14_Unloaded(object sender, RoutedEventArgs e)
        {
            _isNavigating = true;
            _isDisposed = true;
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch { }
        }

        private async Task LoadDataAsync()
        {
            if (_isNavigating || _isDisposed) return;

            CancellationToken token = default;
            try
            {
                if (_cts == null || _cts.IsCancellationRequested) return;
                token = _cts.Token;
            }
            catch { return; }

            var violations = new List<ViolationViewModel>();

            try
            {
                await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return;

                    using (var db = new TrafficSafetyDBContext())
                    {
                        // Lấy dữ liệu hồ sơ vi phạm (Map đúng với cấu trúc 12 bảng mới)
                        var records = db.ViolationRecords.Include(r => r.Law).ToList();

                        foreach (var record in records)
                        {
                            if (token.IsCancellationRequested) return;

                            var vm = new ViolationViewModel
                            {
                                RecordId = record.ViolationRecordId,
                                BienSoXe = record.LicensePlate ?? "",
                                LoiViPham = record.ViolationDetail ?? "Không rõ",
                                IsProcessed = (record.Status == 1),
                                HinhAnh = record.ImagePath ?? ""
                            };

                            if (record.ViolationDate.HasValue)
                            {
                                // Kết hợp Ngày và Giờ
                                var date = record.ViolationDate.Value;
                                var time = record.ViolationTime ?? TimeSpan.Zero;

                                vm.NgayViPham = date.Date.Add(time);
                                vm.ThoiGian = $"{date:dd/MM/yyyy} - {time:hh\\:mm}";
                            }
                            else
                            {
                                vm.NgayViPham = DateTime.MinValue;
                                vm.ThoiGian = "Chưa cập nhật";
                            }

                            violations.Add(vm);
                        }
                    }
                }, token);

                if (!_isNavigating && !_isDisposed && !token.IsCancellationRequested)
                {
                    _allViolations = violations;
                    ApplyFilters();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!_isNavigating && !_isDisposed && !token.IsCancellationRequested)
                {
                    new CustomMessageBox("Lỗi tải dữ liệu vi phạm: " + ex.Message).ShowDialog();
                }
            }
        }

        private void ApplyFilters()
        {
            if (_allViolations == null || dgViolations == null || _isNavigating || _isDisposed) return;

            try
            {
                string keyword = txtSearch?.Text?.Trim().ToLower() ?? "";
                string filterValue = (cmbFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";

                var filtered = _allViolations.AsEnumerable();

                // Lọc theo từ khóa (Biển số hoặc Lỗi)
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = filtered.Where(v => v.BienSoXe.ToLower().Contains(keyword) || v.LoiViPham.ToLower().Contains(keyword));
                }

                // Lọc theo Trạng thái
                if (filterValue == "Chưa xử lý")
                    filtered = filtered.Where(v => !v.IsProcessed);
                else if (filterValue == "Đã xử lý")
                    filtered = filtered.Where(v => v.IsProcessed);

                // Sắp xếp
                IOrderedEnumerable<ViolationViewModel> ordered;
                if (filterValue == "A-Z")
                    ordered = filtered.OrderBy(v => v.LoiViPham).ThenByDescending(v => v.NgayViPham);
                else if (filterValue == "Z-A")
                    ordered = filtered.OrderByDescending(v => v.LoiViPham).ThenByDescending(v => v.NgayViPham);
                else if (filterValue == "Mới nhất")
                    ordered = filtered.OrderByDescending(v => v.NgayViPham);
                else
                {
                    // Mặc định (Tất cả): Ưu tiên Chưa Xử lý lên đầu, sau đó sắp xếp theo ngày
                    if (filterValue == "Tất cả")
                        ordered = filtered.OrderBy(v => v.IsProcessed).ThenByDescending(v => v.NgayViPham);
                    else
                        ordered = filtered.OrderByDescending(v => v.NgayViPham);
                }

                // Đánh lại số thứ tự (STT) cho lưới
                var finalResult = ordered.ToList();
                for (int i = 0; i < finalResult.Count; i++)
                {
                    finalResult[i].Stt = i + 1;
                }

                dgViolations.ItemsSource = finalResult;
            }
            catch { }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isNavigating && !_isDisposed)
            {
                ApplyFilters();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!_isNavigating && !_isDisposed)
            {
                ApplyFilters();
            }
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isNavigating && !_isDisposed)
            {
                ApplyFilters();
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isNavigating && !_isDisposed)
            {
                ApplyFilters();
            }
        }

        private void dgViolations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;

            if (sender is DataGridRow row && row.Item is ViolationViewModel viewModel)
            {
                _isNavigating = true;
                try
                {
                    _cts?.Cancel();
                }
                catch { }

                NavigationService?.Navigate(new Page22(_currentUser as Officer, viewModel.RecordId));
            }
        }

        private void BtnAddViolation_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page23(_currentUser) : new Page23());
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
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(new Page1());
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _isDisposed) return;

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
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        // Chuyển trang LBBVP
        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        //Chuyển trang QuẨn l? tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(new Page1());
        }
    }
}









