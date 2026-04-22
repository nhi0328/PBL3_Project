using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class Page51 : Page
    {
        private readonly Admin _currentUser;
        private readonly LuatItem _currentLuat;

        // Constructor m?c ð?nh
        public Page51()
        {
            InitializeComponent();
            this.Loaded += Page51_Loaded;
        }

        // Constructor chính
        public Page51(LuatItem luat, Admin user) : this()
        {
            _currentLuat = luat;
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page51_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadData();
        }

        private void LoadData()
        {
            if (_currentLuat == null) return;

            txtTenLoi.Text = _currentLuat.TenLoi;
            txtNghiDinh.Text = _currentLuat.CanCu;
            txtNgayBanHanh.Text = _currentLuat.NgayBanHanh;
            txtNgayHieuLuc.Text = _currentLuat.NgayHieuLuc;

            if (_currentLuat.HasPhatTienXeMay)
            {
                spPhatXeMay.Visibility = Visibility.Visible;
                txtPhatXeMay.Text = $"Ph?t ti?n t? {_currentLuat.PhatTienXeMay} ð?i v?i ngý?i ði?u khi?n xe mô tô, xe máy";
            }
            else
            {
                spPhatXeMay.Visibility = Visibility.Collapsed;
            }

            if (_currentLuat.HasPhatTienOto)
            {
                spPhatOto.Visibility = Visibility.Visible;
                txtPhatOto.Text = $"Ph?t ti?n t? {_currentLuat.PhatTienOto} ð?i v?i ngý?i ði?u khi?n xe Ô tô";
            }
            else
            {
                spPhatOto.Visibility = Visibility.Collapsed;
            }

            if (_currentLuat.HasTruDiem)
            {
                spTruDiem.Visibility = Visibility.Visible;
                txtTruDiem.Text = _currentLuat.TruDiem;
            }
            else
            {
                spTruDiem.Visibility = Visibility.Collapsed;
            }
        }

        // --- CÁC HÀM T?M KI?M & L?C ---
        private void btnSearch_Click(object sender, RoutedEventArgs e) { }
        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e) { }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('ð', 'd').Replace('Ð', 'D').ToLower();
        }

        private void FilterLaws()
        {
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { if (_currentUser is Admin admin) { new AdminProfileWindow(admin).ShowDialog(); } }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page44(_currentUser));
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page45(_currentUser));
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page46(_currentUser));
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page47(_currentUser));
        }

        private void btnLichSu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page48(_currentUser));
        }

        private void btnThongKe_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page49(_currentUser));
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page45(_currentUser));
            }
        }

        private void btnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat != null)
            {
                var l = new LuatItem 
                {
                    LawId = _currentLuat.LawId,
                    TenLoi = _currentLuat.TenLoi,
                    PhatTienOto = _currentLuat.PhatTienOto,
                    PhatTienXeMay = _currentLuat.PhatTienXeMay,
                    TruDiem = _currentLuat.TruDiem,
                    CanCu = _currentLuat.CanCu,
                    NgayBanHanh = _currentLuat.NgayBanHanh,
                    NgayHieuLuc = _currentLuat.NgayHieuLuc
                };
                //NavigationService.Navigate(new Page52(l, _currentUser));
            }
        }

        private void btnXoaLuat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLuat == null) return;

            MessageBoxResult result = MessageBox.Show($"B?n có ch?c ch?n mu?n xoá lu?t '{_currentLuat.TenLoi}' không?", "Xác nh?n Xoá", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new TrafficSafetyDBContext())
                    {
                        var lawToDelete = db.TrafficLaws.FirstOrDefault(l => l.LawId == _currentLuat.LawId);

                        if (lawToDelete != null)
                        {
                            db.TrafficLaws.Remove(lawToDelete);
                            db.SaveChanges();

                            new CustomMessageBox("Ð? xoá lu?t thành công.", "Thông báo").ShowDialog();

                            NavigationService.Navigate(new Page45(_currentUser));
                        }
                        else
                        {
                            new CustomMessageBox("Không t?m th?y lu?t trên h? th?ng. Có th? nó ð? b? xóa trý?c ðó.", "L?i").ShowDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("L?i khi Xoá CSDL: " + ex.Message, "L?i").ShowDialog();
                }
            }
        }
    }
}




