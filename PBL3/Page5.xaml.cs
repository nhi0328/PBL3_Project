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
        private ObservableCollection<TrafficLawDto> _lawsList = new ObservableCollection<TrafficLawDto>();

        // Constructor mặc định
        public Page5()
        {
            InitializeComponent();
            LoadData();
        }

        public Page5(Customer user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Bổ sung dòng này
            }
        }

        private void LoadData(string keyword = "")
        {
            using (var _context = new TrafficSafetyDBContext())
            {
                var query = _context.TrafficLaws.Include(t => t.Details).ThenInclude(d => d.Category).AsQueryable();
                
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.LawName.Contains(keyword));
                }

                var laws = query.ToList();
                _lawsList.Clear();

                foreach (var law in laws)
                {
                    var dto = new TrafficLawDto
                    {
                        LawName = law.LawName,
                        DisplayDetails = new List<string>()
                    };

                    foreach (var detail in law.Details)
                    {
                        if (!string.IsNullOrEmpty(detail.FineAmount))
                        {
                            string categoryName = detail.Category != null ? detail.Category.CategoryName : "phương tiện";
                            dto.DisplayDetails.Add($"Phạt tiền {detail.FineAmount} đối với người điều khiển {categoryName}");
                        }
                        if (detail.DemeritPoints.HasValue && detail.DemeritPoints.Value > 0)
                        {
                            dto.DisplayDetails.Add($"Trừ {detail.DemeritPoints} điểm bằng lái xe");
                        }
                    }
                    
                    if(dto.DisplayDetails.Count == 0)
                        dto.DisplayDetails.Add("Chưa có thông tin chi tiết mức phạt");

                    _lawsList.Add(dto);
                }

                icLaws.ItemsSource = _lawsList;
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

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        // Xử lý sự kiện nút Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtIdentifier.Text;
            LoadData(keyword);
        }
    }

    public class TrafficLawDto
    {
        public string LawName { get; set; }
        public List<string> DisplayDetails { get; set; }
    }
}
