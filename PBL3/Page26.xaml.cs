using PBL3.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace PBL3
{
    public partial class Page26 : Page
    {
        // TRẢ LẠI KIỂU INT CHO COMPLAINT ID
        private readonly Officer _currentUser;
        private readonly int _complaintId;

        // Constructor mặc định
        public Page26()
        {
            InitializeComponent();
        }

        // Constructor test không truyền Officer
        public Page26(int complaintId)
        {
            InitializeComponent();
            _complaintId = complaintId;
            LoadData();
        }

        // Constructor chính
        public Page26(Officer user, int complaintId)
        {
            InitializeComponent();
            _currentUser = user;
            _complaintId = complaintId;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }

            LoadData();
        }

        // --- TẢI DỮ LIỆU BẰNG ENTITY FRAMEWORK ---
        private void LoadData()
        {
            if (_complaintId == 0) return;

            try
            {
                using var db = new TrafficSafetyDBContext();
                // So sánh (Hết báo lỗi gạch đỏ)
                var complaint = db.Complaints.FirstOrDefault(c => c.ComplaintId == _complaintId);

                if (complaint != null)
                {
                    txtLoaiPhanAnh.Text = "Phản ánh vi phạm";

                    // Render ngày giờ an toàn
                    txtNgayPhanAnh.Text = complaint.SubmitDate != DateTime.MinValue
                        ? complaint.SubmitDate.ToString("dd/MM/yyyy HH:mm")
                        : "Chưa cập nhật";

                    txtTieuDe.Text = $"Biên bản #{complaint.LicensePlate:D5}";
                    txtNoiDung.Text = complaint.Content ?? "Không có nội dung";

                    // Trạng thái: 0 = Chưa xử lý (Đỏ), 1 = Đã xử lý (Xanh)
                    if (complaint.Status == 0)
                    {
                        txtTrangThai.Text = "Chưa xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
                    }
                    else if (complaint.Status == 1)
                    {
                        txtTrangThai.Text = "Đã xử lý";
                        txtTrangThai.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    }
                    else
                    {
                        txtTrangThai.Text = "Không xác định";
                        txtTrangThai.Foreground = new SolidColorBrush(Colors.Gray);
                    }

                    txtImageName.Text = "Không có hình ảnh đính kèm";

                    // Render nội dung phản hồi (Đổi thành tên cột đúng nếu cần)
                    if (complaint.Status == 1)
                    {
                        txtPhanHoi.Text = complaint.OfficerResponse ?? "";
                        txtPhanHoi.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                    }
                    else
                    {
                        txtPhanHoi.Text = "Chưa có phản hồi...";
                        txtPhanHoi.Foreground = new SolidColorBrush(Colors.Gray);
                    }

                    // Phân quyền tương tác
                    if (complaint.Status == 0)
                    {
                        btnXacNhanXuLy.Visibility = Visibility.Visible;
                        txtPhanHoi.IsReadOnly = false;
                    }
                    else
                    {
                        btnXacNhanXuLy.Visibility = Visibility.Collapsed;
                        txtPhanHoi.IsReadOnly = true;
                    }
                }
                else
                {
                    new CustomMessageBox("Không tìm thấy thông tin phản ánh này.", "Lỗi").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi tải chi tiết phản ánh: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }

        // --- XỬ LÝ Placeholder (Chữ mờ) CHO Ô PHẢN HỒI ---
        private void txtPhanHoi_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!txtPhanHoi.IsReadOnly && (txtPhanHoi.Text == "Nội dung phản hồi" || txtPhanHoi.Text == "Chưa có phản hồi..."))
            {
                txtPhanHoi.Text = "";
                txtPhanHoi.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
        }

        private void txtPhanHoi_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!txtPhanHoi.IsReadOnly && string.IsNullOrWhiteSpace(txtPhanHoi.Text))
            {
                txtPhanHoi.Text = "Chưa có phản hồi...";
                txtPhanHoi.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        // --- NÚT XÁC NHẬN GỬI PHẢN HỒI BẰNG EF CORE ---
        private void btnXacNhanXuLy_Click(object sender, RoutedEventArgs e)
        {
            if (_complaintId == 0) return;

            string responseText = txtPhanHoi.Text.Trim();
            if (responseText == "Nội dung phản hồi" || responseText == "Chưa có phản hồi..." || string.IsNullOrWhiteSpace(responseText))
            {
                new CustomMessageBox("Vui lòng nhập nội dung trả lời trước khi xác nhận xử lý.", "Thiếu thông tin").ShowDialog();
                return;
            }

            try
            {
                using var db = new TrafficSafetyDBContext();
                var complaint = db.Complaints.FirstOrDefault(c => c.ComplaintId == _complaintId);

                if (complaint != null)
                {
                    complaint.Status = 1; // 1 = Đã xử lý
                    complaint.LicensePlate = responseText; // Đổi thành tên cột đúng nếu cần

                    db.SaveChanges();

                    new CustomMessageBox("Đã xác nhận xử lý và gửi phản hồi thành công!", "Thông báo").ShowDialog();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi cập nhật trạng thái: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }

        // --- SỰ KIỆN NÚT "QUAY LẠI" ---
        private void LabelQuayLai_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page16(_currentUser));
        }

        // --- CÁC NÚT MENU VÀ SIDEBAR ---
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
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
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page16(_currentUser));
        private void Button_Click(object sender, RoutedEventArgs e) { }
    }
}