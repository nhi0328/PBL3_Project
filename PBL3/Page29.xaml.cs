using PBL3.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PBL3
{
    public partial class Page29 : Page
    {
        private readonly Customer _currentUser;
        private readonly int _violationId;

        public Page29() { InitializeComponent(); }

        public Page29(Customer user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = (_currentUser as Customer)?.FullName;
                myBell.LoadData(_currentUser as Customer);
            }
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
        public Page29(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        public Page29(Customer user, int violationId) : this(user)
        {
            _violationId = violationId;
            this.Loaded += Page29_Loaded;
        }

        private void Page29_Loaded(object sender, RoutedEventArgs e)
        {
            using var db = new TrafficSafetyDBContext();
            var violation = db.ViolationRecords.FirstOrDefault(v => v.ViolationRecordId == _violationId);
            if (violation != null)
            {
                txtLicensePlate.Text = violation.LicensePlate;
                // Basic binding logic goes here:
                var law = db.TrafficLaws.FirstOrDefault(l => l.LawId == violation.LawId);

                string fullLawName = law != null ? law.LawName : violation.ViolationDescription;
                txtLawName.Text = string.IsNullOrEmpty(fullLawName) ? "Không có dữ liệu" : fullLawName;

                // Handle multiple errors formatting if there are \n or numbered lines in description
                if (!string.IsNullOrEmpty(violation.ViolationDescription) && violation.ViolationDescription.Contains("\n"))
                {
                    txtLawName.Text = violation.ViolationDescription;
                }

                var category = db.Categories.FirstOrDefault(c => c.CategoryId == violation.CategoryId);
                txtCategory.Text = category != null ? category.CategoryName : "Không rõ";

                txtDate.Text = violation.ViolationDate?.ToString("dd/MM/yyyy");
                txtTime.Text = violation.ViolationTime?.ToString(@"hh\:mm");
                txtAddress.Text = violation.Address;
                txtDescription.Text = violation.ViolationDescription;

                // Fine and Demerit formatting
                var details = db.TrafficLawDetails.Where(d => d.LawId == violation.LawId).ToList();
                if (details.Count > 0)
                {
                    var lines = txtLawName.Text.Split('\n');
                    bool multiParts = lines.Length > 1;

                    if (multiParts)
                    {
                        var fineList = new List<string>();
                        var demeritList = new List<string>();

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var d = details.FirstOrDefault(); // If multiple distinct laws map differently we'd query those, but based on single LawId here we apply the same or default to the DB value
                            if (d != null)
                            {
                                fineList.Add($"Lỗi {i + 1}: {d.FineAmount} VNĐ");
                                if (d.DemeritPoints.HasValue) demeritList.Add($"Lỗi {i + 1}: {d.DemeritPoints} Điểm");
                            }
                        }
                        txtFine.Text = string.Join("\n", fineList);
                        txtDemerit.Text = string.Join("\n", demeritList);
                    }
                    else
                    {
                        txtFine.Text = string.Join("\n", details.Select(d => d.FineAmount + " VNĐ"));
                        txtDemerit.Text = string.Join("\n", details.Select(d => d.DemeritPoints.HasValue ? $"{d.DemeritPoints} điểm" : ""));
                    }
                }
                else
                {
                    txtFine.Text = "Không có dữ liệu";
                    txtDemerit.Text = violation.DemeritPoints ?? "Không rõ";
                }

                if (violation.Status == 1 || violation.Status == 2)
                {
                    txtStatus.Text = "Đã xử lý";
                    txtStatus.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#388E3C"));
                }
                else
                {
                    txtStatus.Text = "Chưa xử lý";
                    txtStatus.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C62828"));
                }

                txtLastUpdated.Text = $"Cập nhật lần cuối: {violation.LastUpdate?.ToString("HH:mm dd/MM/yyyy") ?? DateTime.Now.ToString("HH:mm dd/MM/yyyy")}";

                if (!string.IsNullOrEmpty(violation.ImagePath) && System.IO.File.Exists(violation.ImagePath))
                {
                    txtImageName.Text = System.IO.Path.GetFileName(violation.ImagePath);
                    try
                    {
                        imgViolation.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetFullPath(violation.ImagePath)));
                    }
                    catch { imgViolation.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/Images/vf9.png")); }
                }
                else if (!string.IsNullOrEmpty(violation.ImagePath))
                {
                    txtImageName.Text = violation.ImagePath;
                    try
                    {
                        imgViolation.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/Images/" + violation.ImagePath));
                    }
                    catch { imgViolation.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/Images/vf9.png")); }
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4());
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5());
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6());
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7());
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8());
        }
    }
}
