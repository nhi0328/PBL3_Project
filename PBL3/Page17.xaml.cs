using Microsoft.EntityFrameworkCore;
using PBL3.Models;
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

namespace PBL3
{
    public partial class Page17 : Page
    {
        private readonly int? _recordId;

        // Constructor mặc định
        public Page17()
        {
            InitializeComponent();
        }

        public Page17(int recordId)
        {
            InitializeComponent();
            _recordId = recordId;
            LoadViolationDetail();
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
            if (_recordId is null) return;

            try
            {
                using var db = new TrafficSafetyDBContext();

                // 1. DÙNG INCLUDE THẦN THÁNH ĐỂ KÉO ĐẦY ĐỦ BẢNG PHỤ 
                var record = db.ViolationRecords
                    .Include(r => r.Law)
                    .Include(r => r.Category) // Phải có dòng này mới lấy được Loại xe!
                    .Include(r => r.Vehicle).ThenInclude(v => v.VehicleType) // Kéo luôn thông tin gốc của Xe
                    .FirstOrDefault(r => r.ViolationRecordId == _recordId.Value);

                if (record == null)
                {
                    new CustomMessageBox("Không tìm thấy thông tin chi tiết vi phạm này trên hệ thống.").ShowDialog();
                    return;
                }

                // 2. TÌM MỨC PHẠT CHUẨN XÁC TỪ BẢNG CHI TIẾT LUẬT
                string mucPhat = "Chưa có thông tin mức phạt";
                if (record.LawId.HasValue)
                {
                    // Lấy tất cả chi tiết của điều luật này
                    var lawDetails = db.TrafficLawDetails
                                       .Include(d => d.Category)
                                       .Where(d => d.LawId == record.LawId)
                                       .ToList();

                    // Ưu tiên tìm mức phạt khớp với loại phương tiện (Ô tô phạt khác, xe máy phạt khác)
                    var detailMatch = lawDetails.FirstOrDefault(d =>
                        d.Category != null &&
                        record.Category != null &&
                        d.Category.CategoryName == record.Category.CategoryName);

                    // Nếu không tìm được mức phạt riêng, lấy mức phạt chung đầu tiên
                    if (detailMatch == null) detailMatch = lawDetails.FirstOrDefault();

                    if (detailMatch != null && !string.IsNullOrEmpty(detailMatch.FineAmount))
                    {
                        mucPhat = detailMatch.FineAmount;
                    }
                }

                // 3. ĐỔ DỮ LIỆU LÊN GIAO DIỆN (ĐẢM BẢO KHỚP 100% XAML)
                txtHeaderTitle.Text = "BIÊN BẢN VI PHẠM GIAO THÔNG";
                txtHeaderSubtitle.Text = $"Số hồ sơ: {record.ViolationRecordId:D6}";

                // Đổ Loại xe lên (Ưu tiên từ Category, nếu không có lấy từ bảng Vehicle)
                txtVehicleTypeValue.Text = record.Category?.CategoryName ?? record.VehicleBrand ?? "Chưa cập nhật";

                txtViolationDateValue.Text = record.ViolationDate?.ToString("dd/MM/yyyy") ?? "Không rõ";

                // Đổ giờ vi phạm
                txtViolationTimeValue.Text = record.ViolationTime.HasValue
                    ? record.ViolationTime.Value.ToString(@"hh\:mm")
                    : "Không rõ";

                txtViolationLocationValue.Text = record.Address ?? "Không có dữ liệu vị trí";

                txtViolationDescriptionValue.Text = !string.IsNullOrEmpty(record.ViolationDescription)
                    ? record.ViolationDescription
                    : (record.Law?.LawName ?? "Lỗi vi phạm không xác định");

                // Đổ Mức phạt thực tế từ DB lên
                txtFineRangeValue.Text = mucPhat;

                if (txtLicensePointDeductionValue != null)
                {
                    txtLicensePointDeductionValue.Text = (string.IsNullOrEmpty(record.DemeritPoints) || record.DemeritPoints == "0")
                        ? "Không"
                        : record.DemeritPoints;
                }

                txtPaymentLocationValue.Text = "Trụ sở cơ quan CSGT / Cổng DVC Quốc Gia";

                txtStatusValue.Text = record.Status == 1 ? "Đã xử lý / Nộp phạt" : "Chưa xử lý";
                txtStatusValue.Foreground = record.Status == 1 ? Brushes.ForestGreen : Brushes.Firebrick;

                if (txtLastUpdated != null)
                {
                    txtLastUpdated.Text = record.LastUpdate.HasValue
                        ? "Cập nhật lần cuối: " + record.LastUpdate.Value.ToString("dd/MM/yyyy HH:mm")
                        : "Cập nhật lần cuối: Ngay lúc lập biên bản";
                }

                if (txtEvidenceCaption != null)
                {
                    txtEvidenceCaption.Text = "Hình ảnh minh chứng từ hệ thống Camera/Cán bộ";
                }

                // Đổ Hình ảnh lên
                if (!string.IsNullOrWhiteSpace(record.ImagePath) && imgEvidence != null)
                {
                    try
                    {
                        imgEvidence.Source = new BitmapImage(new Uri(record.ImagePath, UriKind.RelativeOrAbsolute));
                        imgEvidence.Visibility = Visibility.Visible;
                        if (txtEvidencePlaceholder != null) txtEvidencePlaceholder.Visibility = Visibility.Collapsed;
                    }
                    catch { /* Lỗi đường dẫn ảnh thì cho qua */ }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi tải dữ liệu từ CSDL: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }
    }
}