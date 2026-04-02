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
        private readonly int? _recordId;

        public Page19()
        {
            InitializeComponent();
        }

        private User _currentUser;

        public Page19(User user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        public Page19(int recordId)
        {
            InitializeComponent();
            _recordId = recordId;
            LoadViolationDetail();
        }

        public Page19(User user, int recordId)
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
            txtPaymentLocationValue.Text = detail.PaymentLocation;
            txtStatusValue.Text = detail.StatusText;
            txtStatusValue.Foreground = detail.IsProcessed ? Brushes.ForestGreen : Brushes.Firebrick;
            txtEvidenceCaption.Text = detail.EvidenceCaption;
            txtLastUpdatedValue.Text = detail.LastUpdated;

            if (!string.IsNullOrWhiteSpace(detail.EvidenceImagePath))
            {
                Uri? evidenceUri = BuildEvidenceUri(detail.EvidenceImagePath);
                if (evidenceUri != null)
                {
                    imgEvidence.Source = new BitmapImage(evidenceUri);
                    imgEvidence.Visibility = Visibility.Visible;
                    txtEvidencePlaceholder.Visibility = Visibility.Collapsed;
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

