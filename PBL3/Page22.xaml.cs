using PBL3.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IOPath = System.IO.Path;

namespace PBL3
{
    public partial class Page22 : Page
    {
        private readonly Officer _currentUser;
        private readonly int? _recordId;

        // Constructor mặc định
        public Page22()
        {
            InitializeComponent();
        }

        // 2. CONSTRUCTOR CHÍNH
        public Page22(Officer user, int recordId)
        {
            InitializeComponent();
            _currentUser = user;
            _recordId = recordId;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }

            LoadViolationDetail();
        }
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page12(_currentUser) : new Page12());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page13(_currentUser) : new Page13());
        }

        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        private void btnNLVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page15(_currentUser) : new Page15());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(_currentUser != null ? new Page16(_currentUser) : new Page16());
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page24());
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        private void LoadViolationDetail()
        {
            if (_recordId is null) return;

            var detail = ViolationLookupService.GetViolationDetail(_recordId.Value);
            if (detail == null)
            {
                new CustomMessageBox("Không tìm thấy thông tin chi tiết vi phạm.").ShowDialog();
                return;
            }

            // Gán dữ liệu lên giao diện
            txtHeaderTitle.Text = detail.HeaderTitle;
            txtHeaderSubtitle.Text = detail.HeaderSubtitle;
            txtVehicleTypeValue.Text = detail.VehicleType;
            txtViolationDateValue.Text = detail.ViolationDate;
            txtViolationTimeValue.Text = detail.ViolationTime;
            txtViolationLocationValue.Text = detail.ViolationLocation;
            txtViolationDescriptionValue.Text = detail.ViolationDescription;
            txtFineRangeValue.Text = detail.FineRange;
            if (txtPointsDeductedValue != null) txtPointsDeductedValue.Text = detail.PointsDeducted;
            txtPaymentLocationValue.Text = detail.PaymentLocation;
            txtStatusValue.Text = detail.StatusText;

            // Đổi màu Trạng thái
            txtStatusValue.Foreground = detail.IsProcessed ? Brushes.ForestGreen : Brushes.Firebrick;
            if (txtEvidenceCaption != null) txtEvidenceCaption.Text = detail.EvidenceCaption;

            // Ẩn hiện các nút chức năng tùy theo trạng thái (Đã xử lý thì không cho sửa nữa)
            if (detail.IsProcessed)
            {
                if (btnConfirm != null) btnConfirm.Visibility = Visibility.Collapsed;
                if (btnEdit != null) btnEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (btnConfirm != null) btnConfirm.Visibility = Visibility.Visible;
                if (btnEdit != null) btnEdit.Visibility = Visibility.Visible;
            }

            // Load ảnh bằng chứng cực kỳ an toàn
            if (!string.IsNullOrWhiteSpace(detail.EvidenceImagePath) && imgEvidence != null)
            {
                Uri? evidenceUri = BuildEvidenceUri(detail.EvidenceImagePath);
                if (evidenceUri != null)
                {
                    try
                    {
                        imgEvidence.Source = new BitmapImage(evidenceUri);
                        imgEvidence.Visibility = Visibility.Visible;

                        if (txtEvidencePlaceholder != null)
                            txtEvidencePlaceholder.Visibility = Visibility.Collapsed;
                    }
                    catch { /* Im lặng bỏ qua nếu ảnh bị lỗi */ }
                }
            }
        }

        private static Uri? BuildEvidenceUri(string evidenceImagePath)
        {
            if (Uri.TryCreate(evidenceImagePath, UriKind.Absolute, out Uri? absoluteUri)) return absoluteUri;

            string fullPath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, evidenceImagePath.TrimStart('/', '\\').Replace('/', IOPath.DirectorySeparatorChar));
            if (File.Exists(fullPath)) return new Uri(fullPath, UriKind.Absolute);

            return Uri.TryCreate(evidenceImagePath, UriKind.Relative, out Uri? relativeUri) ? relativeUri : null;
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_recordId is null) return;

            var result = MessageBox.Show("Xác nhận đánh dấu biên bản này là ĐÃ XỬ LÝ?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;

            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    var record = db.ViolationRecords.FirstOrDefault(r => r.ViolationRecordId == _recordId.Value);
                    if (record != null)
                    {
                        record.Status = 1; // 1 = Đã xử lý
                        db.SaveChanges();
                        new CustomMessageBox("Đã xác nhận xử lý thành công!", "Thông báo").ShowDialog();

                        // Tải lại giao diện ngay lập tức để làm biến mất 2 nút Sửa và Xác nhận
                        LoadViolationDetail();
                    }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi").ShowDialog();
            }
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_recordId is null)
                return;

            NavigationService?.Navigate(_currentUser != null ? new Page23(_currentUser, _recordId.Value) : new Page23(_recordId.Value));
        }
    }
}


