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
        private readonly Officer _currentUser;
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
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
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
        private void btnLogOut_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
            else NavigationService?.Navigate(new Page14(_currentUser));
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) => BtnBack_Click(sender, e);
        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e) { }
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e) { }
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
                var vTypes = db.VehicleTypes
                    .Select(v => v.VehicleTypeName)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                _availableVehicleTypes.Clear();
                _availableVehicleTypes.AddRange(vTypes);
                RefreshVehicleSuggestions(string.Empty, false);

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

        // ================= GỢI Ý CHO LOẠI XE =================
        private void CmbVehicleType_DropDownOpened(object? sender, EventArgs e)
        {
            if (!_isUpdatingVehicleSuggestions) RefreshVehicleSuggestions(cmbVehicleType.Text, false);
        }

        private void CmbVehicleType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingVehicleSuggestions || e.OriginalSource is not TextBox) return;
            RefreshVehicleSuggestions(cmbVehicleType.Text, true);
        }

        private void RefreshVehicleSuggestions(string keyword, bool openDropDown)
        {
            if (_availableVehicleTypes.Count == 0)
            {
                cmbVehicleType.Items.Clear();
                return;
            }

            var matched = string.IsNullOrWhiteSpace(keyword)
                ? _availableVehicleTypes.Take(20).ToList()
                : _availableVehicleTypes.Where(v => v.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

            _isUpdatingVehicleSuggestions = true;
            try
            {
                cmbVehicleType.Items.Clear();
                foreach (string item in matched)
                {
                    cmbVehicleType.Items.Add(new ComboBoxItem { Content = item });
                }

                cmbVehicleType.SelectedItem = null;
                cmbVehicleType.Text = keyword;

                if (GetEditableComboBoxTextBox(cmbVehicleType) is TextBox editableTextBox)
                {
                    editableTextBox.CaretIndex = editableTextBox.Text.Length;
                }

                cmbVehicleType.IsDropDownOpen = openDropDown && matched.Count > 0;
            }
            finally
            {
                _isUpdatingVehicleSuggestions = false;
            }
        }

        private void cmbVehicleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || _isUpdatingVehicleSuggestions) return;
            // Ở đây mình bỏ việc load lại danh sách lỗi theo xe để tránh giật lag, 
            // cán bộ chọn loại xe nào thì vẫn hiển thị full lỗi để họ thoải mái tìm kiếm.
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
            string vehicleTypeName = GetComboBoxText(cmbVehicleType);
            string licensePlate = txtLicensePlate.Text.Trim().ToUpperInvariant();
            string location = txtViolationLocation.Text.Trim();

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

                // 1. TỰ ĐỘNG TÌM HOẶC THÊM MỚI LOẠI XE
                int vehicleTypeId;
                var existingVehicleType = db.VehicleTypes.FirstOrDefault(vt => vt.VehicleTypeName.ToLower() == vehicleTypeName.ToLower());

                if (existingVehicleType != null)
                {
                    vehicleTypeId = existingVehicleType.VehicleTypeId;
                }
                else
                {
                    // Nếu gõ tên loại xe lạ hoắc, tự động thêm mới vào CSDL luôn!
                    var newType = new VehicleType { VehicleTypeName = vehicleTypeName };
                    db.VehicleTypes.Add(newType);
                    db.SaveChanges(); // Lưu ngay để lấy ID tự tăng
                    vehicleTypeId = newType.VehicleTypeId;
                }

                // Fetch vehicle and driving license first (Moved outside the loop for efficiency and scope fix)
                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == licensePlate);
                DrivingLicense? drivingLicense = null;

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
                    record.CategoryId = vehicleTypeId; // Gán ID của loại xe vừa tìm/tạo được
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
                        var detailMatch = lawDetails.FirstOrDefault(d => d.CategoryId == vehicleTypeId);
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