using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PBL3.Models;
using System.Linq; // Thêm thư viện này để dùng LINQ truy vấn Database

namespace PBL3
{
    public partial class AdminProfileWindow : Window
    {
        private Admin _currentAdmin; // Lưu trữ đối tượng Admin hiện tại

        public AdminProfileWindow(Admin admin)
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            _currentAdmin = admin;

            // Load thông tin khi mở cửa sổ
            LoadAdminInfo();
        }

        private void LoadAdminInfo()
        {
            if (_currentAdmin != null)
            {
                txtFullName.Text = string.IsNullOrEmpty(_currentAdmin.FullName) ? "Không xác định" : _currentAdmin.FullName;
                txtAdminId.Text = string.IsNullOrEmpty(_currentAdmin.AdminId) ? "Không có ID" : _currentAdmin.AdminId;
                txtUsername.Text = string.IsNullOrEmpty(_currentAdmin.Username) ? "Không xác định" : _currentAdmin.Username;

                // Load Avatar (giữ nguyên logic cũ của Nhi)
                bool hasImage = false;
                if (!string.IsNullOrEmpty(_currentAdmin.ImagePath))
                {
                    try
                    {
                        string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _currentAdmin.ImagePath.TrimStart('/'));
                        if (File.Exists(imagePath))
                        {
                            imgAvatar.Source = new BitmapImage(new Uri(imagePath));
                            hasImage = true;
                        }
                    }
                    catch
                    {
                        // Ignore load error
                    }
                }

                if (!hasImage)
                {
                    txtNoImage.Visibility = Visibility.Visible;
                }
            }
        }

        // Xử lý nút Đóng (dấu X trên cùng)
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Xử lý nút Hủy
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ TextBox
            string newPassword = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Biến kiểm tra xem Nhi có đang muốn đổi mật khẩu hay không
            // Nếu ô mật khẩu mới KHÔNG rỗng thì nghĩa là Nhi đang muốn đổi
            bool isChangingPassword = !string.IsNullOrWhiteSpace(newPassword);

            try
            {
                using (var db = new TrafficSafetyDBContext()) 
                {
                    var adminInDb = db.Admins.FirstOrDefault(a => a.AdminId == _currentAdmin.AdminId);

                    if (adminInDb != null)
                    {
                        // 2. Nếu có nhập mật khẩu mới thì mới kiểm tra và cập nhật
                        if (isChangingPassword)
                        {
                            if (newPassword != confirmPassword)
                            {
                                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Cập nhật mật khẩu mới vào DB và RAM
                            adminInDb.Password = newPassword;
                            _currentAdmin.Password = newPassword;
                        }

                        // 3. (Tùy chọn) Cập nhật các thông tin khác nếu sau này Nhi thêm ô nhập
                        // ví dụ: adminInDb.FullName = txtFullName.Text;

                        // 4. Ghi Log hệ thống
                        var log = new SystemLog
                        {
                            Id = _currentAdmin.AdminId, // ID người thực hiện
                            Action = 4, // Update
                            TargetPrefix = "A",
                            TargetValue = _currentAdmin.AdminId,
                            Time = DateTime.Now,
                            Role = 1 // Admin
                        };
                        db.SystemLogs.Add(log);

                        db.SaveChanges();

                        string message = isChangingPassword ? "Cập nhật thông tin và đổi mật khẩu thành công!" : "Cập nhật thông tin thành công!";
                        MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}