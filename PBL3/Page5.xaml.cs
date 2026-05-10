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
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace PBL3
{
    public partial class Page5 : Page
    {
        private readonly Customer _currentUser;

        // Constructor mặc định
        public Page5()
        {
            InitializeComponent();
            LoadData();
        }

        public Page5(Customer user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = (_currentUser as Customer)?.FullName;
                myBell.LoadData(_currentUser as Customer);
            }
            LoadData();
        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            // Mở Menu
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page5(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private List<dynamic> _allLaws = new List<dynamic>();

        private void LoadData(string keyword = "")
        {
            using (var db = new TrafficSafetyDBContext())
            {
                var danhSachCategory = db.Categories.ToList();
                var laws = db.TrafficLaws.Select(law => new
                {
                    LawId = law.LawId,
                    LawName = law.LawName,
                    Details = law.Details.ToList()
                }).ToList();

                _allLaws = laws.Select(law =>
                {
                    var detailsList = new List<string>();
                    string searchString = law.LawName ?? "";

                    foreach (var d in law.Details)
                    {
                        string catName = "tất cả phương tiện";
                        if (d.CategoryId.HasValue)
                        {
                            var loaiKhop = danhSachCategory.FirstOrDefault(v => v.CategoryId == d.CategoryId.Value);
                            if (loaiKhop != null) catName = loaiKhop.CategoryName.ToLower();
                        }

                        if (!string.IsNullOrEmpty(d.FineAmount))
                        {
                            if (d.CategoryId == 0)
                            {
                                detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người {catName}");
                            }
                            else
                            {
                                detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với người điều khiển {catName}");
                            }
                            searchString += " " + d.FineAmount + " " + catName;
                        }

                        // Kiểm tra điều kiện trừ điểm
                        if (d.DemeritPoints.HasValue && d.DemeritPoints.Value > 0 && d.CategoryId != 0 && d.CategoryId != 3)
                        {
                            detailsList.Add($"Trừ {d.DemeritPoints.Value} điểm bằng lái đối với người điều khiển {catName}");
                        }
                    }

                    detailsList = detailsList.Distinct().ToList();
                    if (detailsList.Count == 0) detailsList.Add("Chưa có thông tin chi tiết mức phạt");

                    return new
                    {
                        STT = law.LawId,
                        TenLoi = law.LawName, // Đặt là TenLoi để khớp với Binding của XAML (nếu copy từ Admin sang)
                        LawName = law.LawName,
                        Details = detailsList,
                        ChuoiTimKiem = searchString
                    };
                }).Cast<dynamic>().ToList();

                // Lọc nếu có từ khóa
                if (!string.IsNullOrEmpty(keyword))
                {
                    var searchResults = _allLaws
                        .Select(law => new { LawInfo = law, Score = SearchEngine.CalculateScore(law.ChuoiTimKiem, keyword) })
                        .Where(x => x.Score > 0)
                        .OrderByDescending(x => x.Score)
                        .Select(x => x.LawInfo)
                        .ToList();
                    icLaws.ItemsSource = searchResults;
                }
                else
                {
                    icLaws.ItemsSource = _allLaws;
                }
            }
        }

        // Xử lý sự kiện nút Chi tiết
        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.DataContext != null)
            {
                // Lấy dữ liệu của dòng hiện tại
                dynamic law = btn.DataContext;
                int maLuat = law.STT; // STT chính là LawId mình đã map ở trên

                // Truyền User và Mã Luật sang Page27
                NavigationService.Navigate(new Page27(_currentUser as Customer, maLuat));
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6(_currentUser as Customer));
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }
        
        // Xử lý sự kiện nút Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtIdentifier.Text;
            LoadData(keyword);
        }

    }
}
