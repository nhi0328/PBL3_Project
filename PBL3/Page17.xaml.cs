using System;
using System.Collections.Generic;
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
using PBL3.Models;

namespace PBL3
{
    public partial class Page17 : Page
    {
        private readonly int? _recordId;

        public Page17()
        {
            InitializeComponent();
        }

        public Page17(User user)
        {
            InitializeComponent();
        }

        public Page17(int recordId)
        {
            InitializeComponent();
            _recordId = recordId;
            LoadViolationDetail();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        // Xử lý sẽ kiẨn n�t Quay lại
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

            if (!string.IsNullOrWhiteSpace(detail.EvidenceImagePath))
            {
                imgEvidence.Source = new BitmapImage(new Uri(detail.EvidenceImagePath, UriKind.Relative));
                imgEvidence.Visibility = Visibility.Visible;
                txtEvidencePlaceholder.Visibility = Visibility.Collapsed;
            }
        }
    }
}


