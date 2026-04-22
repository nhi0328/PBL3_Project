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

        // Constructor m?c ð?nh (b?t bu?c b?i XAML)
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
                        MessageBox.Show("Không t?m th?y ph?n ánh này!", "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    txtTieuDe.Text = string.IsNullOrEmpty(complaint.Title) ? "Không có tiêu ð?" : complaint.Title;
                    txtNgayGui.Text = complaint.SubmitDate != DateTime.MinValue ? complaint.SubmitDate.ToString("dd/MM/yyyy HH:mm") : "Chýa c?p nh?t";
                    txtNoiDung.Text = string.IsNullOrEmpty(complaint.Content) ? "Không có n?i dung ph?n ánh." : complaint.Content;

                    if (complaint.Status == 0)
                    {
                        txtStatus.Text = "Chýa x? l?";
                        borderStatus.Background = new SolidColorBrush(Color.FromRgb(198, 40, 40)); // #C62828
                    }
                    else
                    {
                        txtStatus.Text = "Ð? x? l?";
                        borderStatus.Background = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // #2E7D32
                    }

                    txtPhanHoi.Text = string.IsNullOrEmpty(complaint.OfficerResponse) ? "Cõ quan ch?c nãng ðang trong quá tr?nh xét duy?t và hoàn thi?n." : complaint.OfficerResponse;

                    // H?nh ?nh
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
                        catch { /* B? qua l?i load ?nh */ }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i t?i chi ti?t ph?n ánh: " + ex.Message);
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("B?n có ch?c ch?n mu?n xóa ph?n ánh này?", "Xác nh?n xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
                                TargetPrefix = "P", // P: Ph?n ánh
                                TargetValue = _complaintId.ToString(),
                                Time = DateTime.Now
                            };
                            db.SystemLogs.Add(log);

                            db.SaveChanges();

                            MessageBox.Show("Ð? xóa ph?n ánh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService.GoBack();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("L?i xóa ph?n ánh: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { if (_currentUser is Admin admin) { new AdminProfileWindow(admin).ShowDialog(); } }
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page44(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page45(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page46(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page47(_currentUser));
        private void btnLichSu_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page48(_currentUser));
        private void btnThongKe_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new Page49(_currentUser));
    }
}




