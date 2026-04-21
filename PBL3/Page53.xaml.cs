using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public partial class Page53 : Page
    {
        private readonly Officer _currentUser;
        private readonly string _targetCccd;

        // Constructor mặc định
        public Page53()
        {
            InitializeComponent();
            this.Loaded += Page53_Loaded;
        }

        // Constructor chính
        public Page53(Officer user, string targetCccd = null) : this()
        {
            _currentUser = user;
            _targetCccd = targetCccd;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
                myBell.LoadData(_currentUser as Officer);
            }
        }

        // Tạm thời thêm lại Constructor phụ cho Page47/Page48 để code không lỗi
        public Page53(Admin admin, int complaintId) : this()
        {
            // Do nothing, Admin pages should navigate to the correct Admin complaint detail page instead.
        }

        private void Page53_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_targetCccd))
            {
                var btn = this.FindName("btnTaoTaiKhoan") as Button;
                if (btn != null) btn.Content = "Cập nhật";
                LoadCustomerData(_targetCccd);
            }
        }

        private void LoadCustomerData(string cccd)
        {
            using (var db = new TrafficSafetyDBContext())
            {
                var customer = db.Customers.FirstOrDefault(c => c.Cccd == cccd);
                if (customer != null)
                {
                    txtHoTen.Text = customer.FullName;
                    txtHoTen.Foreground = Brushes.Black;

                    txtCccd.Text = customer.Cccd;
                    txtCccd.Foreground = Brushes.Black;
                    txtCccd.IsReadOnly = true; 

                    txtNgaySinh.Text = customer.Dob?.ToString("dd/MM/yyyy") ?? "dd/mm/yyyy";
                    if (txtNgaySinh.Text != "dd/mm/yyyy") txtNgaySinh.Foreground = Brushes.Black;

                    if (customer.Gender == "Nam") cmbGioiTinh.SelectedIndex = 1;
                    else if (customer.Gender == "Nữ") cmbGioiTinh.SelectedIndex = 2;

                    txtPhone.Text = customer.Phone;
                    if (!string.IsNullOrEmpty(txtPhone.Text)) txtPhone.Foreground = Brushes.Black;
                    else { txtPhone.Text = "Nhập SĐT"; txtPhone.Foreground = Brushes.Gray; }

                    txtEmail.Text = customer.Email;
                    if (!string.IsNullOrEmpty(txtEmail.Text)) txtEmail.Foreground = Brushes.Black;
                    else { txtEmail.Text = "Nhập Email"; txtEmail.Foreground = Brushes.Gray; }
                }
            }
        }

        private void btnTaoTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    var cccd = txtCccd.Text.Trim();
                    if (string.IsNullOrEmpty(cccd) || cccd == "Nhập CCCD")
                    {
                        MessageBox.Show("Vui lòng nhập CCCD hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var customer = db.Customers.FirstOrDefault(c => c.Cccd == cccd);
                    bool isNew = false;

                    if (customer == null)
                    {
                        customer = new Customer { Cccd = cccd, Password = cccd }; // Default password for new account
                        isNew = true;
                    }

                    customer.FullName = txtHoTen.Text.Trim() == "Nhập họ tên" ? string.Empty : txtHoTen.Text.Trim();

                    if (DateTime.TryParseExact(txtNgaySinh.Text.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dob))
                    {
                        customer.Dob = dob;
                    }

                    if (cmbGioiTinh.SelectedIndex == 1) customer.Gender = "Nam";
                    else if (cmbGioiTinh.SelectedIndex == 2) customer.Gender = "Nữ";

                    customer.Phone = txtPhone.Text.Trim() == "Nhập SĐT" ? null : txtPhone.Text.Trim();
                    customer.Email = txtEmail.Text.Trim() == "Nhập Email" ? null : txtEmail.Text.Trim();

                    if (isNew) db.Customers.Add(customer);

                    db.SaveChanges();

                    // --- BẮT ĐẦU: GHI LOG & TẠO THÔNG BÁO ---
                    int actionType = isNew ? 1 : 2; // 1: Tạo mới, 2: Cập nhật

                    // Lấy ra ID người thực hiện. Ở màn này thường là Cán bộ (Officer) hoặc Quản trị viên (Admin)
                    // (Theo Constructor khai báo thì _currentUser ở đây là Officer)
                    string actorId = _currentUser != null ? _currentUser.OfficerId : "UNKNOWN";
                    int roleType = 2; // 2 = Cán bộ

                    // 1. Ghi vào SYSTEMLOGS
                    var log = new SystemLog
                    {
                        Action = actionType,
                        Id = actorId,
                        Role = roleType,
                        TargetPrefix = "C",
                        TargetValue = cccd,
                        Time = DateTime.Now
                    };
                    db.SystemLogs.Add(log);

                    // 2. Ghi vào NOTIFICATION cho Công dân
                    var noti = new Notification
                    {
                        TargetRole = 3, // 3 = Công dân
                        TargetId = cccd,
                        Content = isNew 
                            ? "Tài khoản của bạn đã được tạo thành công trên không gian số." 
                            : "Thông tin của bạn đã được cập nhật bởi cơ quan chức năng.",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    db.Notifications.Add(noti);

                    db.SaveChanges(); // Lưu lại thay đổi (Log & Notification)
                    // --- KẾT THÚC: GHI LOG & TẠO THÔNG BÁO ---

                    // Tải lại thông báo cho Header/UserControl
                    myBell.LoadData(_currentUser as Officer);

                    MessageBox.Show(isNew ? "Tạo tài khoản thành công!" : "Cập nhật dữ liệu từ bảng CUSTOMERS thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            // Empty placeholder to fix build errors
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && (tb.Text == "Nhập họ tên" || tb.Text == "Nhập CCCD" || tb.Text == "dd/mm/yyyy" || tb.Text == "Nhập SĐT" || tb.Text == "Nhập Email"))
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Foreground = Brushes.Gray;
                if (tb.Name == "txtHoTen") tb.Text = "Nhập họ tên";
                else if (tb.Name == "txtCccd") tb.Text = "Nhập CCCD";
                else if (tb.Name == "txtNgaySinh") tb.Text = "dd/mm/yyyy";
                else if (tb.Name == "txtPhone") tb.Text = "Nhập SĐT";
                else if (tb.Name == "txtEmail") tb.Text = "Nhập Email";
            }
        }

        // Xử lý sự kiện nút Chi tiết
        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ViolationGroupDisplay data)
            {
                try
                {
                    // BẮT BUỘC PHẢI NHÉT `data.RecordId` VÀO TRONG NGOẶC NHƯ VẦY:
                    NavigationService.Navigate(new Page17(data.RecordId));
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("Lỗi khi chuyển trang: " + ex.Message, "Lỗi").ShowDialog();
                }
            }
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page12(_currentUser));
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page13(_currentUser));
        }

        private void btnLBBVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page14(_currentUser));
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page15(_currentUser));
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page16(_currentUser));
        }
    }
}


