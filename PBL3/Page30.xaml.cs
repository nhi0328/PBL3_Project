using PBL3.Models;
using PBL3.ViewModels;
using System;
using System.Linq;
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
    public partial class Page30 : Page
    {
        private readonly Customer _currentUser;

        public Page30() { InitializeComponent(); }

        public Page30(Customer user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = (_currentUser as Customer)?.FullName;
                myBell.LoadData(_currentUser as Customer);
            }
            this.Loaded += Page30_Loaded;
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
        public Page30(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        private void Page30_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                using var db = new TrafficSafetyDBContext();

                // 1. Từ CCCD tìm ra danh sách biển số xe
                var vehicles = db.Vehicles.Where(v => v.Cccd == _currentUser.Cccd).Select(v => v.LicensePlate).ToList();

                // 2. Tìm tất cả các lỗi vi phạm có trừ điểm của các xe đó
                var violations = db.ViolationRecords
                    .Where(v => vehicles.Contains(v.LicensePlate) && v.DemeritPoints != null && v.DemeritPoints != "0")
                    .Join(db.TrafficLaws, v => v.LawId, l => l.LawId, (v, l) => new { v, l })
                    .OrderByDescending(x => x.v.ViolationDate)
                    .Select(x => new
                    {
                        NgayTruDiem = x.v.ViolationDate,
                        SoDiemBiTru = x.v.DemeritPoints,
                        LoiViPham = x.l.LawName ?? "Lỗi không xác định"
                    }).ToList();

                var items = new List<object>();
                int stt = 1;
                int currentPoints = 12; // Base demerit points 

                var backwardsList = violations.OrderBy(v => v.NgayTruDiem).ToList();

                foreach (var v in backwardsList)
                {
                    if (int.TryParse(v.SoDiemBiTru, out int deduction))
                    {
                        currentPoints -= deduction;
                    }
                }

                int displayPoints = currentPoints;
                var finalItems = new List<object>();

                foreach (var v in violations)
                {
                    displayPoints += int.TryParse(v.SoDiemBiTru, out int deduction) ? deduction : 0;

                    finalItems.Add(new
                    {
                        STT = stt++,
                        NgayTruDiem = v.NgayTruDiem?.ToString("dd/MM/yyyy"),
                        SoDiemBiTru = v.SoDiemBiTru,
                        LoiViPham = v.LoiViPham,
                        SoDiemConLai = (displayPoints - (int.TryParse(v.SoDiemBiTru, out int d) ? d : 0)).ToString() // Display points after deduction
                    });
                }

                var dataGrid = this.FindName("dgDemerits") as DataGrid;
                if (dataGrid != null)
                {
                    dataGrid.ItemsSource = finalItems;
                }
            }
        }

        private void btnLuuThongTin_Click(object sender, RoutedEventArgs e)
        {
            // This button handler should not be needed in violation history page
            // If there's a save button in the XAML, its functionality needs to be implemented
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
