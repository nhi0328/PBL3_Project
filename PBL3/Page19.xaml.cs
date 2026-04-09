using PBL3.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// <summary>
    /// Interaction logic for Page19.xaml
    /// </summary>
    public partial class Page19 : Page
    {
        private readonly Officer _currentUser;
        private readonly int? _recordId;

        // Constructor mặc định
        public Page19()
        {
            InitializeComponent();
        }

        // 2. CONSTRUCTOR NHẬN CÁN BỘ & ID BIÊN BẢN (Dùng cái này là chính)
        public Page19(Officer user, int recordId)
        {
            InitializeComponent();
            _currentUser = user;
            _recordId = recordId;

            // Hiển thị tên cán bộ
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }

            // Load thông tin biên bản
            LoadViolationDetail();
        }
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page12());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page13());
        }

        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page14());
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page15());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page16());
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

            // Lấy thông tin từ Service (file ViolationLookupService.cs đã được chốt chuẩn)
            var detail = ViolationLookupService.GetViolationDetail(_recordId.Value);

            if (detail == null)
            {
                new CustomMessageBox("Không tìm thấy thông tin chi tiết vi phạm này trên hệ thống.").ShowDialog();
                return;
            }

            // Gán dữ liệu lên các thành phần giao diện
            txtHeaderTitle.Text = detail.HeaderTitle;
            txtHeaderSubtitle.Text = detail.HeaderSubtitle;
            txtVehicleTypeValue.Text = detail.VehicleType;
            txtViolationDateValue.Text = detail.ViolationDate;
            txtViolationTimeValue.Text = detail.ViolationTime;
            txtViolationLocationValue.Text = detail.ViolationLocation;
            txtViolationDescriptionValue.Text = detail.ViolationDescription;
            txtFineRangeValue.Text = detail.FineRange;
            txtPaymentLocationValue.Text = detail.PaymentLocation;
            txtStatusValue.Text = detail.StatusText;

            // Đổi màu text Trạng thái
            txtStatusValue.Foreground = detail.IsProcessed ? Brushes.ForestGreen : Brushes.Firebrick;

            // Kiểm tra an toàn trước khi gán để tránh lỗi XAML chưa khởi tạo
            if (txtEvidenceCaption != null) txtEvidenceCaption.Text = detail.EvidenceCaption;
            if (txtLastUpdatedValue != null) txtLastUpdatedValue.Text = detail.LastUpdated;

            // Xử lý hiển thị Hình ảnh bằng chứng
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
                    catch { /* Im lặng bỏ qua nếu ảnh bị lỗi file */ }
                }
            }
        }

        private static Uri? BuildEvidenceUri(string evidenceImagePath)
        {
            if (Uri.TryCreate(evidenceImagePath, UriKind.Absolute, out Uri? absoluteUri))
            {
                return absoluteUri;
            }

            string fullPath = IOPath.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                evidenceImagePath.TrimStart('/', '\\').Replace('/', IOPath.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                return new Uri(fullPath, UriKind.Absolute);
            }

            return Uri.TryCreate(evidenceImagePath, UriKind.Relative, out Uri? relativeUri)
                ? relativeUri
                : null;
        }
    }
}

