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
using PBL3.Utils;

namespace PBL3
{
    public partial class Page48 : Page
    {
        private readonly Admin _currentUser;
        private List<LogDisplay> _allLogs = new List<LogDisplay>();

        // Constructor m?c ð?nh
        public Page48()
        {
            InitializeComponent();
            this.Loaded += Page48_Loaded;
        }

        // Constructor chính
        public Page48(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private async void Page48_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                var logs = db.SystemLogs.OrderByDescending(l => l.Time).ToList();
                _allLogs.Clear();

                int stt = 1;
                foreach (var log in logs)
                {
                    string actionStr = $"{TrackingHelper.GetActionName(log.Action)} {TrackingHelper.GetDetailedTargetInfo(log.TargetPrefix, log.TargetValue, db)}";
                    _allLogs.Add(new LogDisplay
                    {
                        STT = stt++,
                        TimeStr = log.Time.ToString("HH:mm dd/MM/yyyy"),
                        RoleName = TrackingHelper.GetRoleName(log.Role),
                        ActorId = log.Id,
                        ActionStr = actionStr,
                        TargetPrefix = log.TargetPrefix,
                        TargetValue = log.TargetValue
                    });
                }

                FilterLogs();
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i l?ch s?: " + ex.Message, "L?i").ShowDialog();
            }
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "T?m ki?m l?ch s?...")
            {
                txtSearch.Text = "";
                txtSearch.Foreground = Brushes.Black;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "T?m ki?m l?ch s?...";
                txtSearch.Foreground = Brushes.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded && txtSearch.Text != "T?m ki?m l?ch s?...")
            {
                FilterLogs();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterLogs();
        }

        private void cbFilterRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                FilterLogs();
            }
        }

        private void FilterLogs()
        {
            if (dgHistory == null || cbFilterRole == null || txtSearch == null) return;

            string keyword = txtSearch.Text.Trim();
            if (keyword == "T?m ki?m l?ch s?...") keyword = "";

            var filtered = _allLogs.AsEnumerable();

            // L?c theo vai tr?
            string roleFilter = (cbFilterRole.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "T?t c?";
            if (roleFilter != "T?t c?")
            {
                filtered = filtered.Where(l => l.RoleName == roleFilter);
            }

            // L?c theo keyword dùng SearchEngine (n?u keyword không r?ng)
            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = filtered.Where(l => 
                    SearchEngine.CalculateScore(l.TimeStr, keyword) > 0 ||
                    SearchEngine.CalculateScore(l.RoleName, keyword) > 0 ||
                    SearchEngine.CalculateScore(l.ActorId, keyword) > 0 ||
                    SearchEngine.CalculateScore(l.ActionStr, keyword) > 0
                ).OrderByDescending(l => Math.Max(
                    Math.Max(SearchEngine.CalculateScore(l.TimeStr, keyword), SearchEngine.CalculateScore(l.RoleName, keyword)),
                    Math.Max(SearchEngine.CalculateScore(l.ActorId, keyword), SearchEngine.CalculateScore(l.ActionStr, keyword))
                ));
            }

            // Gán l?i STT cho danh sách l?c
            var displayList = filtered.ToList();
            for (int i = 0; i < displayList.Count; i++)
            {
                displayList[i].STT = i + 1;
            }

            dgHistory.ItemsSource = displayList;
        }

        private void ActionTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is TextBlock tb && tb.Tag is LogDisplay log)
            {
                if (string.IsNullOrEmpty(log.TargetPrefix)) return;

                string prefix = log.TargetPrefix.ToUpper();
                try
                {
                    if (prefix == "P")
                    {
                        if (int.TryParse(log.TargetValue, out int complaintId))
                        {
                            NavigationService.Navigate(new Page53(_currentUser, complaintId));
                        }
                        else
                        {
                            new CustomMessageBox("L?i d? li?u: M? ph?n ánh không h?p l?!", "L?i").ShowDialog();
                        }
                    }
                    else if (prefix == "L")
                    {
                        NavigationService.Navigate(new Page52(null, _currentUser));
                    }
                    else if (prefix == "O")
                    {
                        NavigationService.Navigate(new Page46(_currentUser));
                    }
                }
                catch (Exception ex)
                {
                    new CustomMessageBox("Chuy?n trang th?t b?i: " + ex.Message).ShowDialog();
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

    public class LogDisplay
    {
        public int STT { get; set; }
        public string TimeStr { get; set; }
        public string RoleName { get; set; }
        public string ActorId { get; set; }
        public string ActionStr { get; set; }
        public string TargetPrefix { get; set; }
        public string TargetValue { get; set; }
    }
}





