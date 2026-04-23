using System;
using System.Windows;
using PBL3.Models; // Đảm bảo using đúng thư mục chứa model của Nhi
using System.Linq;

namespace PBL3
{
    public partial class OfficerProfileWindow : Window
    {
        private Officer _currentOfficer; // Biến lưu trữ tài khoản Cán bộ đang đăng nhập

        public OfficerProfileWindow(Officer officer)
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            _currentOfficer = officer;

            // Load thông tin khi mở cửa sổ
            LoadOfficerInfo();
        }

        private void LoadOfficerInfo()
        {
            if (_currentOfficer != null)
            {
                txtOfficerId.Text = string.IsNullOrEmpty(_currentOfficer.OfficerId) ? "Không có ID" : _currentOfficer.OfficerId;
                txtCCCD.Text = string.IsNullOrEmpty(_currentOfficer.Cccd) ? "Không xác định" : _currentOfficer.Cccd;
            }
        }

        // Tắt bằng dấu X
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Tắt bằng nút Hủy
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Xử lý nút Lưu
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu mật khẩu
            string newPassword = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Kiểm tra xem có đang muốn đổi mật khẩu không
            bool isChangingPassword = !string.IsNullOrWhiteSpace(newPassword);

            try
            {
                using (var db = new TrafficSafetyDBContext()) 
                {
                    // Tìm Cán bộ trong Database
                    var officerInDb = db.Officers.FirstOrDefault(o => o.OfficerId == _currentOfficer.OfficerId);

                    if (officerInDb != null)
                    {
                        // 2. Nếu có nhập mật khẩu mới thì kiểm tra và cập nhật
                        if (isChangingPassword)
                        {
                            if (newPassword != confirmPassword)
                            {
                                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Lưu vào DB và cập nhật trên RAM
                            officerInDb.Password = newPassword;
                            _currentOfficer.Password = newPassword;
                        }

                        // 3. Ghi Log hệ thống
                        var log = new SystemLog
                        {
                            Id = _currentOfficer.OfficerId,
                            Action = 4, // Update
                            TargetPrefix = "O", // O = Officer
                            TargetValue = _currentOfficer.OfficerId,
                            Time = DateTime.Now,
                            Role = 2 // 2 = Officer
                        };
                        db.SystemLogs.Add(log);

                        // 4. Lưu tất cả xuống SQL
                        db.SaveChanges();

                        string message = isChangingPassword ? "Cập nhật thông tin và đổi mật khẩu thành công!" : "Cập nhật thông tin thành công!";
                        MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        this.Close(); // Lưu xong thì tắt popup
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy tài khoản Cán bộ này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}