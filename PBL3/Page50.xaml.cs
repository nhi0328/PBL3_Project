using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public partial class Page50 : Page
    {
        private readonly Admin _currentUser;
        private readonly int? _recordId;

        // Constructor mặc định
        public Page50()
        {
            InitializeComponent();
            this.Loaded += Page50_Loaded;
        }

        // Constructor chính
        public Page50(Admin user, int? recordId = null) : this()
        {
            _currentUser = user;
            _recordId = recordId;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Quản trị viên"; // Hoặc _currentUser.HoTen nếu có
                myBell.LoadData(_currentUser as Admin);
            }
        }

        private async void Page50_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadViolationDetail();
        }

        private void LoadViolationDetail()
        {
            if (_recordId is null) return;

            // Lấy thông tin từ Service (giống Page19)
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
                Uri evidenceUri = BuildEvidenceUri(detail.EvidenceImagePath);
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

        private static Uri BuildEvidenceUri(string evidenceImagePath)
        {
            if (Uri.TryCreate(evidenceImagePath, UriKind.Absolute, out Uri absoluteUri))
            {
                return absoluteUri;
            }

            string fullPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                evidenceImagePath.TrimStart('/', '\\').Replace('/', System.IO.Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
            {
                return new Uri(fullPath, UriKind.Absolute);
            }

            return Uri.TryCreate(evidenceImagePath, UriKind.Relative, out Uri relativeUri)
                ? relativeUri
                : null;
        }

        private void LoadCategories()
        {
            // Do not need to load categories since the combobox is removed
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page44(_currentUser));
            }
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

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page44(_currentUser));
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page45(_currentUser));
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page46(_currentUser));
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page47(_currentUser));
        }

        private void btnLichSu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page48(_currentUser));
        }

        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page49(_currentUser));
        }
    }
}


