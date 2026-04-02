using PBL3.Models;
using System;
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

namespace PBL3
{
    /// <summary>
    /// Interaction logic for Page22.xaml
    /// </summary>
    public partial class Page22 : Page
    {
        private readonly int? _recordId;

        public Page22()
        {
            InitializeComponent();
        }

        private User _currentUser;

        public Page22(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        public Page22(int recordId)
        {
            InitializeComponent();
            _recordId = recordId;
            LoadViolationDetail();
        }

        public Page22(User user, int recordId)
        {
            InitializeComponent();
            _currentUser = user;
            _recordId = recordId;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
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
        }

        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page9());
        }

        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page10());
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
            if (_recordId is null)
                return;

            var detail = ViolationLookupService.GetViolationDetail(_recordId.Value);
            if (detail == null)
            {
                MessageBox.Show("Không tìm thấy thông tin chi tiết vi phạm.");
                return;
            }

            txtHeaderTitle.Text = detail.HeaderTitle;
            txtHeaderSubtitle.Text = detail.HeaderSubtitle;
            txtVehicleTypeValue.Text = detail.VehicleType;
            txtViolationDateValue.Text = detail.ViolationDate;
            txtViolationTimeValue.Text = detail.ViolationTime;
            txtViolationLocationValue.Text = detail.ViolationLocation;
            txtViolationDescriptionValue.Text = detail.ViolationDescription;
            txtFineRangeValue.Text = detail.FineRange;
            txtPointsDeductedValue.Text = detail.PointsDeducted;
            txtPaymentLocationValue.Text = detail.PaymentLocation;
            txtStatusValue.Text = detail.StatusText;
            txtStatusValue.Foreground = detail.IsProcessed ? Brushes.ForestGreen : Brushes.Firebrick;
            txtEvidenceCaption.Text = detail.EvidenceCaption;

            if (detail.IsProcessed)
            {
                btnConfirm.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnConfirm.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(detail.EvidenceImagePath))
            {
                imgEvidence.Source = new BitmapImage(new Uri(detail.EvidenceImagePath, UriKind.Relative));
                imgEvidence.Visibility = Visibility.Visible;
                txtEvidencePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_recordId is null)
                return;

            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    var record = db.ViolationRecords.FirstOrDefault(r => r.Stt == _recordId.Value);
                    if (record != null)
                    {
                        record.Status = 1; // Mark as processed
                        db.SaveChanges();
                        MessageBox.Show("�? Xác nhận Xử lý thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Navigate back or reload the view
                        if (NavigationService?.CanGoBack == true)
                        {
                            NavigationService.GoBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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


