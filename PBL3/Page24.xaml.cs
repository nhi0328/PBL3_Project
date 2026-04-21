using PBL3.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PBL3
{
    public partial class Page24 : Page
    {
        // CHỈ NHẬN OFFICER VÀ MÃ CCCD CỦA CÔNG DÂN CẦN XEM
        private readonly Officer _currentUser;
        private readonly string _targetCccd;

        // Constructor mặc định
        public Page24()
        {
            InitializeComponent();
        }

        // Constructor chính
        public Page24(Officer user, string cccd)
        {
            InitializeComponent();
            _currentUser = user;
            _targetCccd = cccd;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
            this.Loaded += Page24_Loaded;
        }

        // Dành cho test
        public Page24(string cccd)
        {
            InitializeComponent();
            _targetCccd = cccd;
            this.Loaded += Page24_Loaded;
        }

        // --- SỰ KIỆN KHI TRANG LOAD ---
        private async void Page24_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            await LoadDataAsync();
        }

        // --- TẢI DỮ LIỆU CÔNG DÂN BẰNG ENTITY FRAMEWORK ---
        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_targetCccd)) return;

            try
            {
                // Dùng Entity Framework để lấy dữ liệu cực nhanh và an toàn
                using var db = new TrafficSafetyDBContext();
                var customer = await Task.Run(() => db.Customers.FirstOrDefault(c => c.Cccd == _targetCccd));

                if (customer == null)
                {
                    new CustomMessageBox("Không tìm thấy dữ liệu của công dân này trên hệ thống.", "Lỗi").ShowDialog();
                    return;
                }

                // Đổ dữ liệu lên UI
                tbCccd.Text = customer.Cccd;
                tbHoTen.Text = customer.FullName ?? "Chưa cập nhật";
                tbNgaySinh.Text = customer.Dob?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật";
                tbGioiTinh.Text = customer.Gender ?? "Chưa cập nhật";
                tbPhone.Text = customer.Phone ?? "Chưa cập nhật";
                tbEmail.Text = customer.Email ?? "Chưa cập nhật";

                // Load Avatar an toàn
                if (!string.IsNullOrWhiteSpace(customer.Avatar))
                {
                    try
                    {
                        var uri = new Uri(customer.Avatar, UriKind.RelativeOrAbsolute);
                        // Nếu là đường dẫn tương đối, ghép với thư mục chạy phần mềm
                        if (!uri.IsAbsoluteUri)
                        {
                            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customer.Avatar.TrimStart('/', '\\'));
                            if (File.Exists(fullPath)) uri = new Uri(fullPath, UriKind.Absolute);
                        }
                        imgAvatar.Source = new BitmapImage(uri);
                    }
                    catch { /* Im lặng bỏ qua nếu ảnh lỗi */ }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải chi tiết tài khoản: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        // --- XỬ LÝ ĐẶT LẠI MẬT KHẨU CHO CÔNG DÂN ---
        private void pbNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            tbPasswordPlaceholder.Visibility = string.IsNullOrEmpty(pbNewPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void btnLuuMatKhau_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = pbNewPassword.Password;
            if (string.IsNullOrEmpty(newPassword))
            {
                new CustomMessageBox("Vui lòng nhập mật khẩu mới.", "Nhắc nhở").ShowDialog();
                return;
            }

            try
            {
                using var db = new TrafficSafetyDBContext();
                var customer = db.Customers.FirstOrDefault(c => c.Cccd == _targetCccd);

                if (customer != null)
                {
                    // LƯU Ý: Nếu model Customer của Nhi không dùng chữ "Password" mà dùng "MatKhau", 
                    // thì Nhi đổi chữ Password ở dòng dưới này thành MatKhau nhé!
                    customer.Password = newPassword;

                    await db.SaveChangesAsync(); // Cập nhật bằng EF siêu gọn

                    new CustomMessageBox("Cập nhật mật khẩu thành công!", "Thông báo").ShowDialog();
                    pbNewPassword.Password = "";
                }
                else
                {
                    new CustomMessageBox("Không tìm thấy tài khoản để cập nhật.", "Lỗi").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi cập nhật mật khẩu: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG ---
        private void btnChiTietGplx_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page25(_currentUser, _targetCccd));
        }

        private void btnChiTietPhuongTien_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page40(_currentUser, _targetCccd));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page15(_currentUser));
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
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page1());

        // --- SIDEBAR MENU ---
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page16(_currentUser));

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page53(_currentUser, _targetCccd));
        }

        private void btnXoaTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa tài khoản này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var db = new Models.TrafficSafetyDBContext())
                    {
                        var customer = db.Customers.FirstOrDefault(c => c.Cccd == _targetCccd);
                        if (customer != null)
                        {
                            db.Customers.Remove(customer);

                            // --- BẮT ĐẦU: GHI LOG & TẠO THÔNG BÁO ---
                            string actorId = _currentUser != null ? _currentUser.OfficerId : "UNKNOWN";

                            // Ghi lại hành động Xóa (Action = 3)
                            var log = new SystemLog
                            {
                                Action = 3, // 3: Xóa
                                Id = actorId,
                                Role = 2, // 2: Cán bộ
                                TargetPrefix = "C", // C: Công dân
                                TargetValue = _targetCccd,
                                Time = DateTime.Now
                            };
                            db.SystemLogs.Add(log);

                            // Lưu vào db
                            db.SaveChanges();

                            myBell.LoadData(_currentUser as Officer);
                            // --- KẾT THÚC: GHI LOG & TẠO THÔNG BÁO ---

                            MessageBox.Show("Xóa tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService.Navigate(new Page15(_currentUser));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}