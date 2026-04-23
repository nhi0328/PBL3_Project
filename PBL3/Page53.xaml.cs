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

        // Constructor m?c đ?nh
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
                txtUserName.Text = $"Cán b?: {_currentUser.OfficerId}";
                myBell.LoadData(_currentUser as Officer);
            }
        }

        // T?m th?i thêm l?i Constructor ph? cho Page47/Page48 đ? code không l?i
        public Page53(Admin admin, int complaintId) : this()
        {
            // Do nothing, Admin pages should navigate to the correct Admin complaint detail page instead.
        }

        private void Page53_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_targetCccd))
            {
                var btn = this.FindName("btnTaoTaiKhoan") as Button;
                if (btn != null) btn.Content = "C?p nh?t";
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
                    else if (customer.Gender == "N?") cmbGioiTinh.SelectedIndex = 2;

                    txtPhone.Text = customer.Phone;
                    if (!string.IsNullOrEmpty(txtPhone.Text)) txtPhone.Foreground = Brushes.Black;
                    else { txtPhone.Text = "Nh?p SĐT"; txtPhone.Foreground = Brushes.Gray; }

                    txtEmail.Text = customer.Email;
                    if (!string.IsNullOrEmpty(txtEmail.Text)) txtEmail.Foreground = Brushes.Black;
                    else { txtEmail.Text = "Nh?p Email"; txtEmail.Foreground = Brushes.Gray; }
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
                    if (string.IsNullOrEmpty(cccd) || cccd == "Nh?p CCCD")
                    {
                        MessageBox.Show("Vui l?ng nh?p CCCD h?p l?.", "L?i", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var customer = db.Customers.FirstOrDefault(c => c.Cccd == cccd);
                    bool isNew = false;

                    if (customer == null)
                    {
                        customer = new Customer { Cccd = cccd, Password = cccd }; // Default password for new account
                        isNew = true;
                    }

                    customer.FullName = txtHoTen.Text.Trim() == "Nh?p h? tên" ? string.Empty : txtHoTen.Text.Trim();

                    if (DateTime.TryParseExact(txtNgaySinh.Text.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dob))
                    {
                        customer.Dob = dob;
                    }

                    if (cmbGioiTinh.SelectedIndex == 1) customer.Gender = "Nam";
                    else if (cmbGioiTinh.SelectedIndex == 2) customer.Gender = "N?";

                    customer.Phone = txtPhone.Text.Trim() == "Nh?p SĐT" ? null : txtPhone.Text.Trim();
                    customer.Email = txtEmail.Text.Trim() == "Nh?p Email" ? null : txtEmail.Text.Trim();

                    if (isNew) db.Customers.Add(customer);

                    db.SaveChanges();

                    // --- B?T Đ?U: GHI LOG & T?O THÔNG BÁO ---
                    int actionType = isNew ? 1 : 2; // 1: T?o m?i, 2: C?p nh?t

                    // L?y ra ID ngư?i th?c hi?n. ? màn này thư?ng là Cán b? (Officer) ho?c Qu?n tr? viên (Admin)
                    // (Theo Constructor khai báo th? _currentUser ? đây là Officer)
                    string actorId = _currentUser != null ? _currentUser.OfficerId : "UNKNOWN";
                    int roleType = 2; // 2 = Cán b?

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
                            ? "Tài kho?n c?a b?n đ? đư?c t?o thành công trên không gian s?." 
                            : "Thông tin c?a b?n đ? đư?c c?p nh?t b?i cơ quan ch?c năng.",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    db.Notifications.Add(noti);

                    db.SaveChanges(); // Lưu l?i thay đ?i (Log & Notification)
                    // --- K?T THÚC: GHI LOG & T?O THÔNG BÁO ---

                    // T?i l?i thông báo cho Header/UserControl
                    myBell.LoadData(_currentUser as Officer);

                    MessageBox.Show(isNew ? "T?o tài kho?n thành công!" : "C?p nh?t d? li?u t? b?ng CUSTOMERS thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            // Empty placeholder to fix build errors
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && (tb.Text == "Nh?p h? tên" || tb.Text == "Nh?p CCCD" || tb.Text == "dd/mm/yyyy" || tb.Text == "Nh?p SĐT" || tb.Text == "Nh?p Email"))
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
                if (tb.Name == "txtHoTen") tb.Text = "Nh?p h? tên";
                else if (tb.Name == "txtCccd") tb.Text = "Nh?p CCCD";
                else if (tb.Name == "txtNgaySinh") tb.Text = "dd/mm/yyyy";
                else if (tb.Name == "txtPhone") tb.Text = "Nh?p SĐT";
                else if (tb.Name == "txtEmail") tb.Text = "Nh?p Email";
            }
        }

        // X? l? s? ki?n nút Chi ti?t
        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ViolationGroupDisplay data)
            {
                try
                {
                    // B?T BU?C PH?I NHÉT `data.RecordId` VÀO TRONG NGO?C NHƯ V?Y:
                    NavigationService.Navigate(new Page17(data.RecordId));
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("L?i khi chuy?n trang: " + ex.Message, "L?i").ShowDialog();
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new OfficerProfileWindow(_currentUser).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiện cửa sổ: " + ex.Message);
            }
        }

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



