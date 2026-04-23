#nullable enable
using Microsoft.Win32;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using System.Windows.Navigation;
using System.Windows.Threading;
using IOPath = System.IO.Path;

namespace PBL3
{
    public partial class Page23 : Page
    {
        // CHỈ NHẬN OFFICER
        private readonly Officer _currentUser = null!;
        private readonly int? _recordId;

        private string? _selectedEvidenceImagePath;

        // Lists dùng cho Gợi ý (Auto-suggest)
        private readonly List<string> _availableTrafficLawViolations = new();
        private readonly List<string> _availableVehicleTypes = new(); // List chứa loại xe

        private readonly List<string> _selectedViolations = new();

        private bool _isUpdatingViolationSuggestions;
        private bool _isUpdatingVehicleSuggestions;

        // Constructor mặc định
        public Page23()
        {
            InitializeComponent();
            InitializeSearchBoxes();
            LoadFormOptions();
            InitializeFormDefaults();
            UpdateEvidenceSelectionText();
        }

        // Constructor khi tạo mới Biên bản
        public Page23(Officer user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";

                myBell.LoadData(_currentUser as Officer);
            }
        }

        // Constructor khi Chỉnh sửa Biên bản
        public Page23(Officer user, int recordId) : this(user)
        {
            _recordId = recordId;
            LoadViolationDetail();
        }

        // Dành cho lúc test chưa có Officer
        public Page23(int recordId) : this()
        {
            _recordId = recordId;
            LoadViolationDetail();
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG TỪ SIDEBAR MENU ---
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page13(_currentUser));
        private void btnNLVP_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page16(_currentUser));
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
            else NavigationService?.Navigate(new Page14(_currentUser));
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => BtnBack_Click(sender, e);
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
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // --- KHỞI TẠO MẶC ĐỊNH NGÀY GIỜ ---
        private void InitializeFormDefaults()
        {
            if (txtDateDisplay != null) txtDateDisplay.Text = DateTime.Today.ToString("dd/MM/yyyy");
            if (RealDatePicker != null) RealDatePicker.SelectedDate = DateTime.Today;
            if (txtViolationTime != null) txtViolationTime.Text = DateTime.Now.ToString("HH:mm");
        }

        private void RealDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePicker.SelectedDate is DateTime selectedDate)
                txtDateDisplay.Text = selectedDate.ToString("dd/MM/yyyy");
        }

        private void txtDateDisplay_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DateTime.TryParseExact(txtDateDisplay.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime typedDate))
            {
                RealDatePicker.SelectedDate = typedDate;
                txtDateDisplay.Text = typedDate.ToString("dd/MM/yyyy");
            }
        }

        private void DateOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            DateOverlay.IsHitTestVisible = false;
            RealDatePicker.IsHitTestVisible = true;
            RealDatePicker.Opacity = 0.01;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                RealDatePicker.Focus();
                RealDatePicker.IsDropDownOpen = true;
            }), DispatcherPriority.Background);
        }

        private void RealDatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            RealDatePicker.IsHitTestVisible = false;
            RealDatePicker.Opacity = 0;
            DateOverlay.IsHitTestVisible = true;
        }

        // --- XỬ LÝ LẤY DỮ LIỆU LUẬT VÀ LOẠI XE TỪ DATABASE ---
        private void LoadFormOptions()
        {
            try
            {
                using TrafficSafetyDBContext db = new TrafficSafetyDBContext();

                // 1. Tải danh sách Loại xe
                var categories = db.Categories
                                   .Where(c => c.CategoryName != "Đi bộ" && c.CategoryName != "Xe đạp")
                                   .ToList();
                if (cmbVehicleType != null)
                {
                    cmbVehicleType.ItemsSource = categories;
                    cmbVehicleType.DisplayMemberPath = "CategoryName";
                    cmbVehicleType.SelectedValuePath = "CategoryId";
                }

                // Tải danh sách Tỉnh/TP
                if (cboProvince != null)
                {
                    var provinces = db.Provinces.ToList();
                    cboProvince.ItemsSource = provinces;
                    cboProvince.DisplayMemberPath = "ProvinceName";
                    cboProvince.SelectedValuePath = "ProvinceId";
                }

                // 2. Tải danh sách Luật
                List<string> violations = db.TrafficLaws
                    .Select(l => l.LawName)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                _availableTrafficLawViolations.Clear();
                _availableTrafficLawViolations.AddRange(violations);
                RefreshViolationSuggestions(string.Empty, false);

                if (_selectedViolations.Count > 0)
                {
                    _selectedViolations.RemoveAll(v => !_availableTrafficLawViolations.Contains(v));
                    RenderSelectedViolations();
                }
            }
            catch { }
        }

        private static TrafficLaw? FindTrafficLawByName(TrafficSafetyDBContext db, string violationName)
        {
            return db.TrafficLaws.FirstOrDefault(l => l.LawName == violationName);
        }

        // --- ĐĂNG KÝ SỰ KIỆN GỢI Ý CHO COMBOBOX ---
        private void InitializeSearchBoxes()
        {
            // Cho combobox Lỗi vi phạm
            cmbViolationType.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(CmbViolationType_TextChanged));
            cmbViolationType.DropDownOpened += CmbViolationType_DropDownOpened;

            // Cho combobox Loại xe
            cmbVehicleType.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(CmbVehicleType_TextChanged));
            cmbVehicleType.DropDownOpened += CmbVehicleType_DropDownOpened;
        }

        private void cmbVehicleType_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                var tb = cb.Template.FindName("PART_EditableTextBox", cb) as TextBox;
                if (tb != null && tb.IsFocused)
                {
                    string searchText = cb.Text + e.Text;

                    System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(cb.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = item =>
                        {
                            if (string.IsNullOrEmpty(searchText)) return true;

                            string itemName = "";
                            if (cb == cmbVehicleType && item is Category c) itemName = c.CategoryName;
                            else return true;

                            return SearchEngine.CalculateScore(itemName, searchText) > 0;
                        };

                        cb.IsDropDownOpen = true;
                    }
                }
            }
        }

        // ================= GỢI Ý CHO LOẠI XE =================
        private void CmbVehicleType_DropDownOpened(object? sender, EventArgs e)
        {
        }

        private void CmbVehicleType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OriginalSource is not TextBox tb || !tb.IsFocused) return;
            
            string searchText = cmbVehicleType.Text;
            System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(cmbVehicleType.ItemsSource);
            if (view != null)
            {
                view.Filter = item =>
                {
                    if (string.IsNullOrEmpty(searchText)) return true;
                    if (item is Category c) return SearchEngine.CalculateScore(c.CategoryName, searchText) > 0;
                    return true;
                };

                cmbVehicleType.IsDropDownOpen = true;
            }
        }

        private void cboVehicleType_LostFocus(object sender, RoutedEventArgs e)
        {
            AddNewVehicleTypeIfNeeded();
        }

        private void cboVehicleType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNewVehicleTypeIfNeeded();
                cmbVehicleType.IsDropDownOpen = false;
            }
        }

        private void AddNewVehicleTypeIfNeeded()
        {
            string newTypeSearch = cmbVehicleType.Text.Trim();
            if (string.IsNullOrEmpty(newTypeSearch)) return;

            using var db = new TrafficSafetyDBContext();
            bool exists = db.Categories.Any(c => c.CategoryName.ToLower() == newTypeSearch.ToLower());
            if (!exists)
            {
                var newCat = new Category { CategoryName = newTypeSearch };
                db.Categories.Add(newCat);
                db.SaveChanges();

                // Refresh loại xe
                var categories = db.Categories
                                   .Where(c => c.CategoryName != "Đi bộ" && c.CategoryName != "Xe đạp")
                                   .ToList();
                cmbVehicleType.ItemsSource = categories;
                cmbVehicleType.DisplayMemberPath = "CategoryName";
                cmbVehicleType.SelectedValuePath = "CategoryId";
                cmbVehicleType.Text = newTypeSearch; // Giữ lại text vừa nhập
            }
        }

        private void btnConfirmPlate_Click(object sender, RoutedEventArgs e)
        {
            txtLicensePlateStatus.Visibility = Visibility.Collapsed;
            string plate = txtLicensePlate.Text.Trim();
            if (string.IsNullOrEmpty(plate)) return;

            using var db = new TrafficSafetyDBContext();
            bool exists = db.Vehicles.Any(v => v.LicensePlate == plate);

            if (!exists)
            {
                txtLicensePlateStatus.Text = $"Biển số xe {plate} chưa được đăng ký.";
                txtLicensePlateStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                txtLicensePlateStatus.Visibility = Visibility.Visible;
            }
            else
            {
                txtLicensePlateStatus.Text = $"Biển số xe hợp lệ.";
                txtLicensePlateStatus.Foreground = new SolidColorBrush(Colors.Green);
                txtLicensePlateStatus.Visibility = Visibility.Visible;
            }
        }

        private void cboProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProvince.SelectedValue is int provinceId && cboWard != null)
            {
                using var db = new TrafficSafetyDBContext();
                try
                {
                    var property = typeof(TrafficSafetyDBContext).GetProperty("Wards");
                    if (property != null)
                    {
                        IEnumerable<dynamic>? wardsDbSet = property.GetValue(db) as IEnumerable<dynamic>;
                        if (wardsDbSet != null)
                        {
                            var wards = wardsDbSet.Where(w => w.ProvinceId == provinceId).ToList();
                            cboWard.ItemsSource = wards;
                            cboWard.DisplayMemberPath = "WardName";
                            cboWard.SelectedValuePath = "WardId";
                        }
                    }
                }
                catch { }
            }
        }

        private void CmbProvince_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                var tb = cb.Template.FindName("PART_EditableTextBox", cb) as TextBox;
                if (tb != null && tb.IsFocused)
                {
                    string searchText = cb.Text;

                    System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(cb.ItemsSource);
                    if (view != null)
                    {
                        view.Filter = item =>
                        {
                            if (string.IsNullOrEmpty(searchText)) return true;

                            string itemName = "";
                            if (cb == cboProvince && item is Province p) itemName = p.ProvinceName;
                            else if (cb == cboWard && item is Ward w) itemName = w.WardName;
                            else return true;

                            return SearchEngine.CalculateScore(itemName, searchText) > 0;
                        };

                        cb.IsDropDownOpen = true;
                    }
                }
            }
        }

        // ================= GỢI Ý CHO LỖI VI PHẠM =================
        private void CmbViolationType_DropDownOpened(object? sender, EventArgs e)
        {
            if (!_isUpdatingViolationSuggestions) RefreshViolationSuggestions(cmbViolationType.Text, false);
        }

        private void cmbViolationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingViolationSuggestions || cmbViolationType.SelectedItem is not ComboBoxItem item) return;
            AddSelectedViolation(item.Content?.ToString());
        }

        private void btnAddViolation_Click(object sender, RoutedEventArgs e)
        {
            TryAddViolationFromCurrentText();
        }

        private void cmbViolationType_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryAddViolationFromCurrentText();
                e.Handled = true;
            }
        }

        private void CmbViolationType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingViolationSuggestions || e.OriginalSource is not TextBox) return;
            RefreshViolationSuggestions(cmbViolationType.Text, true);
        }

        private void RefreshViolationSuggestions(string keyword, bool openDropDown)
        {
            if (_availableTrafficLawViolations.Count == 0)
            {
                cmbViolationType.Items.Clear();
                return;
            }

            List<string> matchedViolations = GetFilteredViolations(keyword);
            _isUpdatingViolationSuggestions = true;

            try
            {
                cmbViolationType.Items.Clear();
                foreach (string item in matchedViolations)
                {
                    cmbViolationType.Items.Add(new ComboBoxItem { Content = item });
                }

                cmbViolationType.SelectedItem = null;
                cmbViolationType.Text = keyword;

                if (GetEditableComboBoxTextBox(cmbViolationType) is TextBox editableTextBox)
                {
                    editableTextBox.CaretIndex = editableTextBox.Text.Length;
                }

                cmbViolationType.IsDropDownOpen = openDropDown && matchedViolations.Count > 0;
            }
            finally
            {
                _isUpdatingViolationSuggestions = false;
            }
        }

        private List<string> GetFilteredViolations(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return _availableTrafficLawViolations.Take(50).ToList();
            string normalizedKeyword = NormalizeSearchText(keyword);

            return _availableTrafficLawViolations
                .Select(item => new { Value = item, Score = GetSearchScore(item, normalizedKeyword) })
                .Where(item => item.Score < int.MaxValue)
                .OrderBy(item => item.Score)
                .Take(20)
                .Select(item => item.Value)
                .ToList();
        }

        private static int GetSearchScore(string text, string normalizedKeyword)
        {
            string normalizedText = NormalizeSearchText(text);
            if (normalizedText.Equals(normalizedKeyword, StringComparison.OrdinalIgnoreCase)) return 0;
            if (normalizedText.StartsWith(normalizedKeyword, StringComparison.OrdinalIgnoreCase)) return 1;
            if (normalizedText.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase)) return 2;

            int distance = ComputeLevenshteinDistance(normalizedText, normalizedKeyword);
            if (distance <= 3) return 10 + distance;

            return int.MaxValue;
        }

        private static string NormalizeSearchText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            string result = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            builder.Clear();
            foreach (char character in result)
            {
                builder.Append(character switch
                {
                    'đ' => 'd',
                    'Đ' => 'd',
                    _ => character
                });
            }

            return Regex.Replace(builder.ToString(), "\\s+", " ").Trim();
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int[,] matrix = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++) matrix[i, 0] = i;
            for (int j = 0; j <= target.Length; j++) matrix[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[source.Length, target.Length];
        }

        private static TextBox? GetEditableComboBoxTextBox(ComboBox comboBox)
        {
            return comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
        }

        private void TryAddViolationFromCurrentText()
        {
            string currentText = cmbViolationType.Text.Trim();
            if (string.IsNullOrWhiteSpace(currentText)) return;

            string? match = _availableTrafficLawViolations.FirstOrDefault(v => string.Equals(v, currentText, StringComparison.OrdinalIgnoreCase));
            if (match != null) AddSelectedViolation(match);
        }

        private void AddSelectedViolation(string? violation)
        {
            if (string.IsNullOrWhiteSpace(violation)) return;

            if (_selectedViolations.Any(v => string.Equals(v, violation, StringComparison.OrdinalIgnoreCase)))
            {
                cmbViolationType.Text = string.Empty;
                RefreshViolationSuggestions(string.Empty, false);
                return;
            }

            _selectedViolations.Add(violation);
            RenderSelectedViolations();
            cmbViolationType.Text = string.Empty;
            RefreshViolationSuggestions(string.Empty, false);
        }

        private void RenderSelectedViolations()
        {
            SelectedViolationsPanel.Children.Clear();

            foreach (string violation in _selectedViolations)
            {
                Border chip = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDECEC")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C72421")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Margin = new Thickness(0, 0, 8, 8),
                    Padding = new Thickness(10, 6, 8, 6)
                };

                StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = violation,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 16,
                    Foreground = Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center
                });

                Button removeButton = new Button
                {
                    Content = "✕",
                    Tag = violation,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C72421")),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(8, 0, 0, 0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                removeButton.Click += RemoveSelectedViolation_Click;

                stackPanel.Children.Add(removeButton);
                chip.Child = stackPanel;
                SelectedViolationsPanel.Children.Add(chip);
            }
        }

        private void RemoveSelectedViolation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string violation)
            {
                _selectedViolations.RemoveAll(v => string.Equals(v, violation, StringComparison.OrdinalIgnoreCase));
                RenderSelectedViolations();
            }
        }

        // --- XỬ LÝ HÌNH ẢNH BẰNG CHỨNG ---
        private void UploadEvidenceArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "Chọn ảnh vi phạm"
            };

            if (dialog.ShowDialog() != true) return;

            _selectedEvidenceImagePath = CopyEvidenceFileToStorage(dialog.FileName);
            UpdateEvidenceSelectionText();
        }

        private static string CopyEvidenceFileToStorage(string sourceFilePath)
        {
            string destinationDirectory = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "EvidenceImages");
            Directory.CreateDirectory(destinationDirectory);

            string extension = IOPath.GetExtension(sourceFilePath);
            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            string destinationPath = IOPath.Combine(destinationDirectory, fileName);

            File.Copy(sourceFilePath, destinationPath, true);
            return IOPath.Combine("EvidenceImages", fileName).Replace('\\', '/');
        }

        private void UpdateEvidenceSelectionText()
        {
            if (string.IsNullOrWhiteSpace(_selectedEvidenceImagePath))
            {
                txtEvidenceFileName.Text = "+ Tải hình ảnh lên";
                txtEvidenceStatus.Text = "Tối đa 5 ảnh";
                imgEvidencePreview.Source = null;
                imgEvidencePreview.Visibility = Visibility.Collapsed;
                txtEvidencePreviewPlaceholder.Visibility = Visibility.Visible;
                return;
            }

            txtEvidenceFileName.Text = IOPath.GetFileName(_selectedEvidenceImagePath);
            txtEvidenceStatus.Text = "Nhấn để chọn lại ảnh";

            Uri? previewUri = BuildEvidencePreviewUri(_selectedEvidenceImagePath);
            if (previewUri != null)
            {
                imgEvidencePreview.Source = new BitmapImage(previewUri);
                imgEvidencePreview.Visibility = Visibility.Visible;
                txtEvidencePreviewPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private static Uri? BuildEvidencePreviewUri(string evidenceImagePath)
        {
            if (Uri.TryCreate(evidenceImagePath, UriKind.Absolute, out Uri? absoluteUri)) return absoluteUri;

            string fullPath = IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, evidenceImagePath.TrimStart('/', '\\').Replace('/', IOPath.DirectorySeparatorChar));
            if (File.Exists(fullPath)) return new Uri(fullPath, UriKind.Absolute);

            return Uri.TryCreate(evidenceImagePath, UriKind.Relative, out Uri? relativeUri) ? relativeUri : null;
        }

        private static string GetComboBoxText(ComboBox comboBox)
        {
            if (!string.IsNullOrWhiteSpace(comboBox.Text)) return comboBox.Text.Trim();
            if (comboBox.SelectedItem is ComboBoxItem item) return item.Content?.ToString()?.Trim() ?? string.Empty;
            return comboBox.Text?.Trim() ?? string.Empty;
        }

        private static void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            if (comboBox.ItemsSource != null)
            {
                foreach (object item in comboBox.Items)
                {
                    string? itemStr = null;
                    if (!string.IsNullOrEmpty(comboBox.DisplayMemberPath))
                    {
                        var prop = item.GetType().GetProperty(comboBox.DisplayMemberPath);
                        if (prop != null) itemStr = prop.GetValue(item)?.ToString();
                    }
                    else
                    {
                        itemStr = item.ToString();
                    }

                    if (string.Equals(itemStr, value, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox.SelectedItem = item;
                        return;
                    }
                }
                comboBox.Text = value; // Editable ComboBox support without corrupting ItemsSource
            }
            else
            {
                foreach (object item in comboBox.Items)
                {
                    if (item is ComboBoxItem comboBoxItem && string.Equals(comboBoxItem.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox.SelectedItem = comboBoxItem;
                        return;
                    }
                }
                comboBox.Items.Add(new ComboBoxItem { Content = value });
                comboBox.SelectedIndex = comboBox.Items.Count - 1;
            }
        }

        // --- LOAD CHI TIẾT VI PHẠM KHI CHỈNH SỬA ---
        private void LoadViolationDetail()
        {
            if (_recordId is null) return;

            var detail = ViolationLookupService.GetViolationDetail(_recordId.Value);
            if (detail == null)
            {
                new CustomMessageBox("Không tìm thấy thông tin chi tiết vi phạm.").ShowDialog();
                return;
            }

            SetComboBoxSelection(cmbVehicleType, detail.VehicleType == "Ô tô" ? "Xe Ô tô" : detail.VehicleType);

            txtLicensePlate.Text = detail.HeaderTitle.Replace("Phương tiện ", string.Empty).Trim();

            _selectedViolations.Clear();
            _selectedViolations.Add(detail.ViolationDescription);
            RenderSelectedViolations();

            cmbViolationType.Text = string.Empty;
            txtDateDisplay.Text = detail.ViolationDate;

            if (DateTime.TryParseExact(detail.ViolationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
            {
                RealDatePicker.SelectedDate = selectedDate;
            }

            txtViolationTime.Text = detail.ViolationTime;
            txtViolationLocation.Text = detail.ViolationLocation;
            _selectedEvidenceImagePath = detail.EvidenceImagePath;
            UpdateEvidenceSelectionText();
        }

        // --- NÚT LƯU BIÊN BẢN (THÊM/SỬA LOẠI XE TỰ ĐỘNG) ---
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string vehicleTypeName = cmbVehicleType.Text.Trim();
            string licensePlate = txtLicensePlate.Text.Trim().ToUpperInvariant();
            string location = txtViolationLocation.Text.Trim();

            int? categoryId = null;

            if (string.IsNullOrWhiteSpace(vehicleTypeName) || string.IsNullOrWhiteSpace(licensePlate) || string.IsNullOrWhiteSpace(location))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin vi phạm (Loại xe, Biển số, Địa điểm).", "Thiếu thông tin");
                return;
            }

            if (_selectedViolations.Count == 0)
            {
                new CustomMessageBox("Vui lòng chọn ít nhất một Lỗi vi phạm.", "Thiếu thông tin").ShowDialog();
                return;
            }

            if (!DateTime.TryParseExact(txtDateDisplay.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime violationDate))
            {
                new CustomMessageBox("Ngày vi phạm không hợp lệ. Vui lòng nhập định dạng dd/MM/yyyy.", "Lỗi nhập liệu").ShowDialog();
                return;
            }

            RealDatePicker.SelectedDate = violationDate;

            if (!TimeSpan.TryParseExact(txtViolationTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan violationTime))
            {
                new CustomMessageBox("Giờ vi phạm không hợp lệ. Vui lòng nhập định dạng HH:mm.", "Lỗi nhập liệu").ShowDialog();
                return;
            }

            try
            {
                using TrafficSafetyDBContext db = new TrafficSafetyDBContext();

                if (!string.IsNullOrEmpty(vehicleTypeName))
                {
                    var cat = db.Categories.FirstOrDefault(c => c.CategoryName.ToLower() == vehicleTypeName.ToLower());
                    if (cat != null) categoryId = cat.CategoryId;
                }

                // Địa chỉ
                int? selectedWardId = null;
                if (cboWard != null && cboWard.SelectedValue != null)
                {
                    selectedWardId = (int)cboWard.SelectedValue;
                }

                // Combine selected laws, delimited by ';'
                var lawIds = new List<int>();
                foreach (string violationStr in _selectedViolations)
                {
                    TrafficLaw? tLaw = FindTrafficLawByName(db, violationStr);
                    if (tLaw != null)
                    {
                        lawIds.Add(tLaw.LawId);
                    }
                }

                string combinedLawIds = string.Join(";", lawIds);

                // Initialize tracking logic and models here ... 
                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == licensePlate);
                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        LicensePlate = licensePlate,
                        VehicleTypeId = categoryId,
                        Status = 1
                    };
                    db.Vehicles.Add(vehicle);
                }

                // Prepare drivingLicense here before passing later
                Models.DrivingLicense? drivingLicense = null;

                if (vehicle != null)
                {
                    drivingLicense = db.DrivingLicenses.FirstOrDefault(d => d.Cccd == vehicle.Cccd);
                }

                // 2. LƯU CÁC LỖI VI PHẠM (Hỗ trợ 1 xe dính nhiều lỗi)
                for (int index = 0; index < _selectedViolations.Count; index++)
                {
                    string violationName = _selectedViolations[index];
                    TrafficLaw? law = FindTrafficLawByName(db, violationName);

                    ViolationRecord record;
                    // Nếu là chế độ Chỉnh sửa
                    if (_recordId.HasValue && index == 0)
                    {
                        record = db.ViolationRecords.FirstOrDefault(v => v.ViolationRecordId == _recordId.Value) ?? new ViolationRecord();
                        if (record.ViolationRecordId == 0) db.ViolationRecords.Add(record);
                    }
                    else
                    {
                        record = new ViolationRecord();
                        db.ViolationRecords.Add(record);
                    }

                    record.LicensePlate = licensePlate;
                    record.CategoryId = categoryId; // Gán ID của loại xe vừa tìm/tạo được
                    record.LawId = law?.LawId ?? 0;
                    record.ViolationDescription = violationName;
                    record.ViolationDate = violationDate.Date;
                    record.ViolationTime = violationTime;
                    record.Address = location;
                    record.Status = 0; // Mặc định là 0 (Chưa xử lý)
                    record.ImagePath = _selectedEvidenceImagePath;

                    // Tính điểm trừ
                    string points = "0";
                    if (law != null)
                    {
                        var lawDetails = db.TrafficLawDetails
                            .Include(d => d.Category)
                            .Where(d => d.LawId == law.LawId)
                            .ToList();
                        var detailMatch = lawDetails.FirstOrDefault(d => d.CategoryId == categoryId);
                        if (detailMatch != null)
                        {
                            points = detailMatch.DemeritPoints?.ToString() ?? "0";
                            if (points == "Tước bằng")
                            {
                                if (drivingLicense != null)
                                {
                                    drivingLicense.Status = 2;
                                    drivingLicense.Points = 0;
                                    drivingLicense.DemeritPoints = 12;
                                }
                            }
                            else if (points == "Tạm giữ xe")
                            {
                                points = "0";
                                if (vehicle != null)
                                {
                                    vehicle.Status = 2;
                                    db.Vehicles.Update(vehicle);
                                }
                            }
                            else if (int.TryParse(points, out int demeritPointsValue) && drivingLicense != null)
                            {
                                if (demeritPointsValue > drivingLicense.Points)
                                {
                                    drivingLicense.Status = 2;
                                    drivingLicense.Points = 0;
                                    drivingLicense.DemeritPoints = 12;
                                }
                                else
                                {
                                    drivingLicense.Points -= demeritPointsValue;
                                    drivingLicense.DemeritPoints += demeritPointsValue;
                                }
                            }
                        }
                        record.DemeritPoints = points;
                    }

                    db.SaveChanges();

                    var newLog = new SystemLog
                    {
                        Role = 2,                              // 2: Cán bộ (Officer)
                        Id = _currentUser.OfficerId,
                        Action = _recordId.HasValue ? 2 : 1,
                        TargetPrefix = "B",
                        TargetValue = record.ViolationRecordId.ToString(),
                        Time = DateTime.Now
                    };
                    db.SystemLogs.Add(newLog);
                    db.SaveChanges();

                    new CustomMessageBox("Đã lưu biên bản vi phạm vào hệ thống thành công!", "Thành công").ShowDialog();
                    NavigationService?.Navigate(new Page14(_currentUser));
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Không thể lưu vi phạm: " + ex.Message, "Lỗi kết nối").ShowDialog();
            }
        }
    }
}
