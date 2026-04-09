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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PBL3
{
    public partial class Page11 : Page
    {
        private List<dynamic> _allLaws = new List<dynamic>();
        // Constructor mặc định
        public Page11()
        {
            InitializeComponent();
            this.Loaded += Page11_Loaded;
        }

        private void Page11_Loaded(object sender, RoutedEventArgs e)
        {
            using var db = new TrafficSafetyDBContext();

            // 1. KÉO BẢNG CATEGORIES LÊN RAM TRƯỚC
            var danhSachCategory = db.Categories.ToList();

            // 2. KÉO BẢNG LUẬT VÀ CHI TIẾT LUẬT LÊN
            var laws = db.TrafficLaws.Select(law => new
            {
                LawId = law.LawId,
                LawName = law.LawName,
                Details = law.Details.ToList()
            }).ToList();

            _allLaws = laws.Select(law =>
            {
                var detailsList = new List<string>();
                string searchString = law.LawName;

                foreach (var d in law.Details)
                {
                    string catName = "Tất cả phương tiện";
                    if (d.CategoryId.HasValue)
                    {
                        var loaiKhop = danhSachCategory.FirstOrDefault(v => v.CategoryId == d.CategoryId.Value);
                        if (loaiKhop != null)
                        {
                            catName = loaiKhop.CategoryName;
                        }
                    }

                    if (!string.IsNullOrEmpty(d.FineAmount))
                    {
                        detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển xe {catName.ToLower()}");
                        searchString += " " + d.FineAmount + " " + catName;
                    }
                    if (d.DemeritPoints.HasValue && d.DemeritPoints.Value > 0)
                    {
                        detailsList.Add($"Trừ {d.DemeritPoints.Value} điểm bằng lái xe");
                    }
                }

                // Loại bỏ các dòng trùng lặp
                detailsList = detailsList.Distinct().ToList();

                return new
                {
                    STT = law.LawId,
                    Name = law.LawName,
                    Details = detailsList,
                    ChuoiTimKiem = searchString
                };
            }).Cast<dynamic>().ToList();

            // Đổ dữ liệu đã ánh xạ hoàn chỉnh lên list
            icLaws.ItemsSource = _allLaws; 
        }

        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                // Trả lại toàn bộ danh sách nếu ô tìm kiếm trống
                icLaws.ItemsSource = _allLaws;
                return;
            }

            // Áp dụng thuật toán chấm điểm trên "ChuoiTimKiem" (đã chứa dữ liệu cả 2 bảng)
            var searchResults = _allLaws
                .Select(law => new
                {
                    LawInfo = law,
                    // Thuật toán sẽ quét cả lỗi vi phạm, loại xe và mức phạt
                    Score = SearchEngine.CalculateScore(law.ChuoiTimKiem, keyword)
                })
                .Where(x => x.Score > 0) // Điểm > 0 mới cho hiển thị
                .OrderByDescending(x => x.Score) // Xếp giống nhất lên đầu
                .Select(x => x.LawInfo)
                .ToList();

            // Cập nhật kết quả cực mượt lên ItemsControl
            icLaws.ItemsSource = searchResults;
        }

        // Xử lý sự kiện nút Quay lại
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int lawId)
            {
                NavigationService.Navigate(new Page18(lawId));
            }
        }
    }
}
