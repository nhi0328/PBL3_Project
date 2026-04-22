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
    public partial class Page46 : Page
    {
        private readonly Admin _currentUser;

        // Constructor m?c ð?nh
        public Page46()
        {
            InitializeComponent();
            this.Loaded += Page46_Loaded;
        }

        // Constructor chính
        public Page46(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName; // Ho?c _currentUser.HoTen n?u có

                myBell.LoadData(_currentUser as Admin);
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<OfficerVM> _officersData = new System.Collections.ObjectModel.ObservableCollection<OfficerVM>();

        private async void Page46_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadOfficers();
        }

        private void LoadOfficers(string keyword = "")
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                var query = db.Officers.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(o => o.OfficerId.Contains(keyword) || o.Cccd.Contains(keyword) || o.Password.Contains(keyword));
                }

                var list = query.ToList();
                _officersData.Clear();
                int stt = 1;
                foreach (var o in list)
                {
                    _officersData.Add(new OfficerVM
                    {
                        STT = stt++,
                        OriginalOfficerId = o.OfficerId,
                        OfficerId = o.OfficerId,
                        Cccd = o.Cccd,
                        Password = o.Password
                    });
                }
                dgOfficers.ItemsSource = _officersData;
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i t?i danh sách: " + ex.Message, "L?i").ShowDialog();
            }
        }

        private void dgOfficers_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Bý?c 1: Ch?n DataGrid không cho nó "nu?t" s? ki?n lãn chu?t
            e.Handled = true;

            // Bý?c 2: T?o m?t s? ki?n lãn chu?t m?i y h?t
            var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;

            // Bý?c 3: Ð?y s? ki?n ðó lên cho th?ng cha c?a nó (chính là cái ScrollViewer)
            var parent = ((Control)sender).Parent as UIElement;
            if (parent != null)
            {
                parent.RaiseEvent(eventArg);
            }
        }

        private void BtnSearchOfficer_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            LoadOfficers(keyword);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            LoadOfficers(keyword);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _officersData.Add(new OfficerVM
            {
                STT = _officersData.Count + 1,
                OriginalOfficerId = null,
                OfficerId = "",
                Cccd = "",
                Password = ""
            });

            dgOfficers.ScrollIntoView(_officersData.Last());
            dgOfficers.Focus();
        }

        private void dgOfficers_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Triggers before RowEditEnding
        }

        private void dgOfficers_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is OfficerVM vm)
                {
                    // Delay to allow DataGrid to finish writing binding values to ViewModel
                    Dispatcher.BeginInvoke(new Action(() => SaveOfficer(vm)), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        private void SaveOfficer(OfficerVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.OfficerId) || string.IsNullOrWhiteSpace(vm.Cccd) || string.IsNullOrWhiteSpace(vm.Password))
            {
                new CustomMessageBox("Vui l?ng ði?n ð?y ð? thông tin (S? hi?u, CCCD, Password).", "Thông báo").ShowDialog();
                return;
            }

            try
            {
                using var db = new TrafficSafetyDBContext();
                if (vm.IsNew)
                {
                    if (db.Officers.Any(x => x.OfficerId == vm.OfficerId))
                    {
                        new CustomMessageBox("S? hi?u này ð? t?n t?i!", "L?i").ShowDialog();
                        return;
                    }
                    var newOfficer = new Officer { OfficerId = vm.OfficerId, Cccd = vm.Cccd, Password = vm.Password };
                    db.Officers.Add(newOfficer);
                    db.SaveChanges();
                    vm.OriginalOfficerId = vm.OfficerId;
                }
                else
                {
                    if (vm.OriginalOfficerId != vm.OfficerId)
                    {
                        if (db.Officers.Any(x => x.OfficerId == vm.OfficerId))
                        {
                            new CustomMessageBox("S? hi?u (m?i) này ð? t?n t?i!", "L?i").ShowDialog();
                            return;
                        }
                        var oldOfficer = db.Officers.Find(vm.OriginalOfficerId);
                        if (oldOfficer != null)
                        {
                            db.Officers.Remove(oldOfficer);
                            db.Officers.Add(new Officer { OfficerId = vm.OfficerId, Cccd = vm.Cccd, Password = vm.Password });
                            db.SaveChanges();
                            vm.OriginalOfficerId = vm.OfficerId;
                        }
                    }
                    else
                    {
                        var exist = db.Officers.Find(vm.OriginalOfficerId);
                        if (exist != null)
                        {
                            exist.Cccd = vm.Cccd;
                            exist.Password = vm.Password;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("L?i khi lýu d? li?u: " + (ex.InnerException?.Message ?? ex.Message), "L?i").ShowDialog();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OfficerVM vm)
            {
                var confirm = new ConfirmDeleteBox();
                if (confirm.ShowDialog() == true)
                {
                    try
                    {
                        if (!vm.IsNew)
                        {
                            using var db = new TrafficSafetyDBContext();
                            var o = db.Officers.Find(vm.OriginalOfficerId);
                            if (o != null)
                            {
                                db.Officers.Remove(o);
                                db.SaveChanges();
                            }
                        }
                        _officersData.Remove(vm);
                        // C?p nh?t l?i STT
                        for (int i = 0; i < _officersData.Count; i++)
                        {
                            _officersData[i].STT = i + 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        new CustomMessageBox("L?i khi xóa: " + ex.Message, "L?i").ShowDialog();
                    }
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

    public class OfficerVM : System.ComponentModel.INotifyPropertyChanged
    {
        private int _stt;
        public int STT { get => _stt; set { _stt = value; OnPropertyChanged(nameof(STT)); } }

        public string OriginalOfficerId { get; set; }

        private string _officerId;
        public string OfficerId { get => _officerId; set { _officerId = value; OnPropertyChanged(nameof(OfficerId)); } }

        private string _cccd;
        public string Cccd { get => _cccd; set { _cccd = value; OnPropertyChanged(nameof(Cccd)); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(nameof(Password)); } }

        public bool IsNew => string.IsNullOrEmpty(OriginalOfficerId);

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
    }
}




