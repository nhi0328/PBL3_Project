using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace PBL3
{
    public class ViolationViewModel
    {
        public int Stt { get; set; }
        public int RecordId { get; set; }
        public string BienSoXe { get; set; }
        public string HangXe { get; set; } // C?t m?i
        public string LoiViPham { get; set; }
        public string ThoiGian { get; set; }
        public string HinhAnh { get; set; } // C?t m?i (ch?a path Ẩnh)
        public bool IsProcessed { get; set; }
        public DateTime NgayViPham { get; set; }
    }

    public partial class Page14 : Page
    {
        private User _currentUser;
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

        public Page14(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
            this.Loaded += Page14_Loaded;
            this.Unloaded += Page14_Unloaded;
        }

        // Constructor nhận thông tin User
        public Page14(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private async void Page14_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            _isNavigating = false;
            _isDisposed = false;
            
            // Properly dispose old CancellationTokenSource before creating new one
            if (_cts != null)
            {
                try
                {
                    _cts.Cancel();
                }
                catch { }
                
                try
                {
                    _cts.Dispose();
                }
                catch { }
            }
            
            _cts = new CancellationTokenSource();
            
            try
            {
                await LoadDataAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected during navigation
            }
            catch (Exception)
            {
                // Handle other exceptions silently during load
            }
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
                if (_cts == null || _cts.IsCancellationRequested)
                    return;
                    
                token = _cts.Token;
            }
            catch
            {
                return;
            }

            var violations = new List<ViolationViewModel>();

            try
            {
                await Task.Run(() =>
                {
                    if (token.IsCancellationRequested) return;

                    using (TrafficSafetyDBContext db = new TrafficSafetyDBContext())
                    {
                        var records = db.ViolationRecords.ToList();

                        // Group violations by LicensePlate, Date, and Time
                        var groupedRecords = records.GroupBy(r => new
                        {
                            LicensePlate = r.LicensePlate ?? "",
                            Date = r.ViolationDate ?? DateTime.MinValue,
                            Time = r.ViolationTime ?? TimeSpan.Zero
                        });

                        foreach (var group in groupedRecords)
                        {
                            if (token.IsCancellationRequested) return;

                            var vm = new ViolationViewModel();
                            
                            // Use the first record's ID as the main ID for the group
                            var firstRecord = group.First();
                            vm.RecordId = firstRecord.Stt;
                            vm.BienSoXe = group.Key.LicensePlate;

                            // Combine descriptions using newline for multiline display
                            vm.LoiViPham = string.Join("\n", group.Select(r => r.ViolationDescription ?? ""));

                            if (group.Key.Date != DateTime.MinValue)
                            {
                                vm.NgayViPham = group.Key.Date.Date.Add(group.Key.Time);
                                vm.ThoiGian = $"{group.Key.Date:dd/MM/yyyy} - {group.Key.Time:hh\\:mm}";
                            }
                            else
                            {
                                vm.NgayViPham = DateTime.MinValue;
                                vm.ThoiGian = "";
                            }

                            // If all records in group are processed (Status == 1), mark as processed
                            vm.IsProcessed = group.All(r => r.Status == 1);

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
            catch (OperationCanceledException)
            {
                // Task was canceled, this is expected during navigation - do nothing
            }
            catch (Exception ex)
            {
                if (!_isNavigating && !_isDisposed && !token.IsCancellationRequested)
                {
                    try
                    {
                        MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
                    }
                    catch { }
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

                // L?c theo t?m ki?m
                if (!string.IsNullOrEmpty(keyword))
                {
                    filtered = filtered.Where(v => v.BienSoXe.ToLower().Contains(keyword) || v.LoiViPham.ToLower().Contains(keyword));
                }

                // L?c trạng thái
                if (filterValue == "Chưa xử lý")
                {
                    filtered = filtered.Where(v => !v.IsProcessed);
                }
                else if (filterValue == "Đã xử lý")
                {
                    filtered = filtered.Where(v => v.IsProcessed);
                }

                IOrderedEnumerable<ViolationViewModel> ordered;

                // S?p x?p
                if (filterValue == "A-Z")
                {
                    ordered = filtered.OrderBy(v => v.LoiViPham).ThenByDescending(v => v.NgayViPham);
                }
                else if (filterValue == "Z-A")
                {
                    ordered = filtered.OrderByDescending(v => v.LoiViPham).ThenByDescending(v => v.NgayViPham);
                }
                else if (filterValue == "M?i nh?t")
                {
                    ordered = filtered.OrderByDescending(v => v.NgayViPham);
                }
                else // Tất cả, Chưa xử lý, Đã xử lý (mặc định)
                {
                    // Ưu tiên Chưa Xử lý lên trước n?u chẨn "Tất cả"
                    if (filterValue == "Tất cả")
                    {
                        ordered = filtered.OrderBy(v => v.IsProcessed).ThenByDescending(v => v.NgayViPham);
                    }
                    else
                    {
                        ordered = filtered.OrderByDescending(v => v.NgayViPham);
                    }
                }

                // Đánh lại sẽ thứ tự (STT)
                var finalResult = ordered.ToList();
                for (int i = 0; i < finalResult.Count; i++)
                {
                    finalResult[i].Stt = i + 1;
                }

                if (!_isNavigating && !_isDisposed)
                {
                    dgViolations.ItemsSource = finalResult;
                }
            }
            catch (Exception)
            {
                // Silently handle exceptions during navigation/disposal
            }
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

        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            
            if (sender is Button button && button.Tag is int recordId)
            {
                _isNavigating = true;
                try
                {
                    _cts?.Cancel();
                }
                catch { }
                
                NavigationService?.Navigate(_currentUser != null ? new Page22(_currentUser, recordId) : new Page22(recordId));
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

        private void dgViolations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page());
        }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _isDisposed) return;
            _isNavigating = true;
            try
            {
                _cts?.Cancel();
            }
            catch { }
            
            NavigationService?.Navigate(new Page9());
        }
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Page10());
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









