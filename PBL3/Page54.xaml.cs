using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public partial class Page54 : Page
    {
        private readonly Admin _currentUser;
        private readonly int _complaintId;

        // Constructor mặc định (bắt buộc bởi XAML)
        public Page54()
        {
            InitializeComponent();
            this.Loaded += Page54_Loaded;
        }

        // Constructor chính
        public Page54(Admin user, int complaintId) : this()
        {
            _currentUser = user;
            _complaintId = complaintId;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
                myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page54_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadComplaintData();
        }

        private void LoadComplaintData()
        {
            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    var complaint = db.Complaints.FirstOrDefault(c => c.ComplaintId == _complaintId);

                    if (complaint == null)
                    {
                        MessageBox.Show("Không tìm thấy phản ánh này!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    txtTieuDe.Text = string.IsNullOrEmpty(complaint.Title) ? "Không có tiêu đề" : complaint.Title;
                    txtNgayGui.Text = complaint.SubmitDate != DateTime.MinValue ? complaint.SubmitDate.ToString("dd/MM/yyyy HH:mm") : "Chưa cập nhật";
                    txtNoiDung.Text = string.IsNullOrEmpty(complaint.Content) ? "Không có nội dung phản ánh." : complaint.Content;

                    if (complaint.Status == 0)
                    {
                        txtStatus.Text = "Chưa xử lý";
                        borderStatus.Background = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // #C62828
                    }
                    else
                    {
                        txtStatus.Text = "Đã xử lý";
                        borderStatus.Background = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // #2E7D32
                    }

                    txtPhanHoi.Text = string.IsNullOrEmpty(complaint.OfficerResponse) ? "Cơ quan chức năng đang trong quá trình xét duyệt và hoàn thiện." : complaint.OfficerResponse;

                    // Hình ảnh
                    if (!string.IsNullOrEmpty(complaint.ImagePath))
                    {
                        try
                        {
                            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, complaint.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                imgPhanAnh.Source = new BitmapImage(new Uri(imagePath));
                            }
                        }
                        catch { /* Bỏ qua lỗi load ảnh */ }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết phản ánh: " + ex.Message);
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phản ánh này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new TrafficSafetyDBContext())
                    {
                        var complaint = db.Complaints.Find(_complaintId);
                        if (complaint != null)
                        {
                            db.Complaints.Remove(complaint);

                            // Ghi log
                            var log = new SystemLog
                            {
                                Action = 3, // 3: Xóa
                                Id = _currentUser != null ? _currentUser.Username : "ADMIN",
                                Role = 1, // 1: Admin
                                TargetPrefix = "P", // P: Phản ánh
                                TargetValue = _complaintId.ToString(),
                                Time = DateTime.Now
                            };
                            db.SystemLogs.Add(log);

                            db.SaveChanges();

                            MessageBox.Show("Đã xóa phản ánh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService.GoBack();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa phản ánh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new Page47(_currentUser));
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
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page44(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page45(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page46(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page47(_currentUser));
        private void btnLichSu_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page48(_currentUser));
        private void btnThongKe_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page49(_currentUser));
    }
}



