using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using PBL3.Models;

namespace PBL3
{
    public partial class Page18 : Page
    {
        private readonly int _lawId;

        // Constructor mặc định
        public Page18()
        {
            InitializeComponent();
        }

        // 2. Constructor nhận ID Luật (Được gọi khi bấm "Xem chi tiết" ở Page11)
        public Page18(int lawId)
        {
            InitializeComponent();
            _lawId = lawId;

            // Gọi hàm load dữ liệu lên giao diện
            this.Loaded += Page18_Loaded;
        }

        private void Page18_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLawDetail();
        }

        // --- SỰ KIỆN NÚT QUAY LẠI ---
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        // --- HÀM ĐỔ DỮ LIỆU LÊN GIAO DIỆN XAML ---
        private void LoadLawDetail()
        {
            if (_lawId <= 0) return;

            using var db = new TrafficSafetyDBContext();
            var law = db.TrafficLaws.Find(_lawId);
            if (law == null) return;

            txtLawName.Text = law.LawName;

            var danhSachCategory = System.Linq.Enumerable.ToList(db.Categories);
            var details = System.Linq.Enumerable.ToList(db.TrafficLawDetails.Where(d => d.LawId == _lawId));

            string decree = "";
            DateTime? issueDate = null;
            DateTime? effectiveDate = null;
            var detailsList = new System.Collections.Generic.List<string>();

            foreach(var d in details)
            {
                if (!string.IsNullOrEmpty(d.Decree) && string.IsNullOrEmpty(decree))
                {
                    decree = d.Decree;
                }
                if (d.IssueDate.HasValue && !issueDate.HasValue)
                {
                    issueDate = d.IssueDate;
                }
                if (d.EffectiveDate.HasValue && !effectiveDate.HasValue)
                {
                    effectiveDate = d.EffectiveDate;
                }

                string catName = "tất cả phương tiện";
                if (d.CategoryId.HasValue)
                {
                    var cat = danhSachCategory.FirstOrDefault(c => c.CategoryId == d.CategoryId.Value);
                    if (cat != null) catName = cat.CategoryName.ToLower();
                }

                if (!string.IsNullOrEmpty(d.FineAmount))
                {
                    detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển {catName}");
                }
                if (d.DemeritPoints.HasValue && d.DemeritPoints > 0)
                {
                    detailsList.Add($"Trừ {d.DemeritPoints} điểm bằng lái xe");
                }
            }

            txtDecree.Text = !string.IsNullOrEmpty(decree) ? decree : "Chưa có thông tin";

            if (issueDate.HasValue)
            {
                txtIssueDate.Text = $"Ngày ban hành: {issueDate.Value:dd/MM/yyyy}";
                txtIssueDate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtIssueDate.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (effectiveDate.HasValue)
            {
                txtEffectiveDate.Text = $"Ngày có hiệu lực: {effectiveDate.Value:dd/MM/yyyy}";
                txtEffectiveDate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtEffectiveDate.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Lấy ra các trường hợp duy nhất
            icPunishments.ItemsSource = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(detailsList));
        }
    }
}