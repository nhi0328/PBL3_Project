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
    public partial class Page45 : Page
    {
        private readonly Admin _currentUser;
        private ObservableCollection<Page45LuatItem> lstLuat = new ObservableCollection<Page45LuatItem>();

        public Page45(Admin user)
        {
            InitializeComponent();
            _currentUser = user;
            if (_currentUser != null) txtUserName.Text = _currentUser.FullName;

            this.Loaded += (s, e) => LoadData();
        }

        private void Page45_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                var categories = db.Categories.ToList();
                var rawLaws = db.TrafficLaws.Include(l => l.Details).ToList();

                var results = new List<Page45LuatItem>();

                foreach (var law in rawLaws)
                {
                    var detailsList = new List<string>();
                    string searchContent = law.LawName ?? "";

                    foreach (var d in law.Details)
                    {
                        string catName = "tất cả phương tiện";
                        if (d.CategoryId.HasValue)
                        {
                            var cat = categories.FirstOrDefault(c => c.CategoryId == d.CategoryId.Value);
                            if (cat != null) catName = cat.CategoryName.ToLower();
                        }

                        if (!string.IsNullOrEmpty(d.FineAmount))
                        {
                            detailsList.Add($"Phạt tiền từ {d.FineAmount} đối với {catName}");
                            searchContent += $" {d.FineAmount} {catName}";
                        }

                        // Không trừ điểm cho xe đạp (ID=3) hoặc không xác định (ID=0)
                        if (d.DemeritPoints.HasValue && d.DemeritPoints > 0 && d.CategoryId != 0 && d.CategoryId != 3)
                        {
                            detailsList.Add($"Trừ {d.DemeritPoints} điểm bằng lái đối với {catName}");
                        }
                    }

                    // Lấy log mới nhất
                    var lastLog = db.SystemLogs
                                    .Where(log => log.TargetPrefix == "L" && log.TargetValue == law.LawId.ToString())
                                    .OrderByDescending(log => log.Time).FirstOrDefault();

                    results.Add(new Page45LuatItem
                    {
                        LawId = law.LawId,
                        TenLoi = law.LawName,
                        Details = detailsList.Distinct().ToList(),
                        ChuoiTimKiem = searchContent,
                        OriginalLaw = law
                    });
                }

                lstLuat = new ObservableCollection<Page45LuatItem>(results);
                dgvDanhSachLuat.ItemsSource = lstLuat;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load: " + ex.Message); }
        }

        // --- CÁC HÀM TÌM KIẾM & LỌC ---
        private void btnSearch_Click(object sender, RoutedEventArgs e) => FilterLaws();
        private void txtIdentifier_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lstLuat == null || dgvDanhSachLuat == null) return;
            string keyword = txtIdentifier.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                dgvDanhSachLuat.ItemsSource = lstLuat;
                return;
            }

            // Dùng SearchEngine để xếp hạng kết quả cho xịn
            var searchResults = lstLuat
                .Select(law => new { Law = law, Score = SearchEngine.CalculateScore(law.ChuoiTimKiem, keyword) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Law).ToList();

            dgvDanhSachLuat.ItemsSource = searchResults;
        }

        private void FilterLaws()
        {
            if (lstLuat == null || dgvDanhSachLuat == null) return;

            string keyword = txtIdentifier.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(keyword))
            {
                dgvDanhSachLuat.ItemsSource = lstLuat;
                return;
            }

            var searchResults = lstLuat
                .Select(law => new
                {
                    LawInfo = law,
                    Score = SearchEngine.CalculateScore(law.ChuoiTimKiem, keyword)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.LawInfo)
                .ToList();

            dgvDanhSachLuat.ItemsSource = searchResults;
        }

        private void btnThemLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page52(null, _currentUser));
        }

        private void btnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Page45LuatItem selected)
            {
                NavigationService.Navigate(new Page51(selected, _currentUser));
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
    }

    public class Page45LuatItem
    {
        public int LawId { get; set; }
        public string TenLoi { get; set; }
        public List<string> Details { get; set; }
        public string ChuoiTimKiem { get; set; }
        public string LastUpdateInfo { get; set; }
        public TrafficLaw OriginalLaw { get; set; }
    }
}




