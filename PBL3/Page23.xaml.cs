using Microsoft.Win32;
using PBL3.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IOPath = System.IO.Path;

namespace PBL3
{
    /// <summary>
    /// Interaction logic for Page23.xaml
    /// </summary>
    public partial class Page23 : Page
    {
        private readonly int? _recordId;
        private string? _selectedEvidenceImagePath;
        private readonly List<string> _availableTrafficLawViolations = new();
        private readonly List<string> _selectedViolations = new();
        private bool _isUpdatingViolationSuggestions;

        public Page23()
        {
            InitializeComponent();
            InitializeViolationTypeSearch();
            LoadFormOptions();
            InitializeFormDefaults();
            UpdateEvidenceSelectionText();
        }

        private User _currentUser;

        public Page23(User user)
        {
            InitializeComponent();
            InitializeViolationTypeSearch();
            LoadFormOptions();
            InitializeFormDefaults();
            UpdateEvidenceSelectionText();
            _currentUser = user;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }
        }

        public Page23(int recordId)
        {
            InitializeComponent();
            InitializeViolationTypeSearch();
            LoadFormOptions();
            InitializeFormDefaults();
            UpdateEvidenceSelectionText();
            _recordId = recordId;
            LoadViolationDetail();
        }

        public Page23(User user, int recordId)
        {
            InitializeComponent();
            InitializeViolationTypeSearch();
            LoadFormOptions();
            InitializeFormDefaults();
            UpdateEvidenceSelectionText();
            _currentUser = user;
            _recordId = recordId;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }

            LoadViolationDetail();
        }

        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page12());
        }

        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page13());
        }

        private void btnNLVP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page14());
        }

        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page15());
        }

        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page16());
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
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
        }

        private void MenuAdminUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page9());
        }

        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page10());
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Page1());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string vehicleType = GetComboBoxText(cmbVehicleType);
            string licensePlate = txtLicensePlate.Text.Trim().ToUpperInvariant();
            string location = txtViolationLocation.Text.Trim();

            if (string.IsNullOrWhiteSpace(vehicleType)
                || string.IsNullOrWhiteSpace(licensePlate)
                || string.IsNullOrWhiteSpace(location))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin vi phạm.");
                return;
            }

            if (_selectedViolations.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một Lỗi vi phạm.");
                return;
            }

            if (!DateTime.TryParseExact(txtDateDisplay.Text.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime violationDate))
            {
                MessageBox.Show("Ngày vi phạm không hợp lệ. Vui lòng nh?p theo định dạng dd/MM/yyyy.");
                return;
            }

            RealDatePicker.SelectedDate = violationDate;

            if (!TimeSpan.TryParseExact(txtViolationTime.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan violationTime))
            {
                MessageBox.Show("Gi? vi phạm không hợp lệ. Vui lòng nh?p theo định dạng HH:mm.");
                return;
            }

            try
            {
                using TrafficSafetyDBContext db = new TrafficSafetyDBContext();
                ViolationLookupService.EnsureEvidenceImagePathColumn(db);

                Vehicle? vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == licensePlate);
                string ownerName = "Chưa cập nhật";

                if (vehicle != null && !string.IsNullOrWhiteSpace(vehicle.OwnerId))
                {
                    ownerName = db.Users
                        .Where(u => u.Cccd == vehicle.OwnerId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? ownerName;
                }

                for (int index = 0; index < _selectedViolations.Count; index++)
                {
                    string violation = _selectedViolations[index];
                    TrafficLaw? law = FindTrafficLawForVehicleType(db, violation, vehicleType);

                    ViolationRecord record;
                    if (_recordId.HasValue && index == 0)
                    {
                        record = db.ViolationRecords.FirstOrDefault(v => v.Stt == _recordId.Value) ?? new ViolationRecord();

                        if (record.Stt == 0)
                        {
                            db.ViolationRecords.Add(record);
                        }
                    }
                    else
                    {
                        record = new ViolationRecord();
                        db.ViolationRecords.Add(record);
                    }

                    record.OwnerName = ownerName;
                    record.LicensePlate = licensePlate;
                    record.VehicleBrand = vehicleType;
                    record.LawId = law?.LawId ?? 0;
                    record.ViolationDescription = violation;
                    record.ViolationDate = violationDate.Date;
                    record.ViolationTime = violationTime;
                    record.DemeritPoints = 0;
                    record.DetailedLocation = location;
                    record.Status = 0;
                    record.EvidenceImagePath = _selectedEvidenceImagePath;
                }

                db.SaveChanges();

                MessageBox.Show("Đã lưu vi phạm vào cơ sẽ dữ liệu.");
                NavigationService?.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu vi phạm: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
                return;
            }

            NavigationService?.Navigate(_currentUser != null ? new Page14(_currentUser) : new Page14());
        }

        private void InitializeFormDefaults()
        {
            if (txtDateDisplay != null)
            {
                txtDateDisplay.Text = DateTime.Today.ToString("dd/MM/yyyy");
            }

            if (RealDatePicker != null)
            {
                RealDatePicker.SelectedDate = DateTime.Today;
            }

            if (txtViolationTime != null)
            {
                txtViolationTime.Text = DateTime.Now.ToString("HH:mm");
            }
        }

        private void RealDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealDatePicker.SelectedDate is not DateTime selectedDate)
            {
                return;
            }

            string formattedDate = selectedDate.ToString("dd/MM/yyyy");
            txtDateDisplay.Text = formattedDate;
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

        private void LoadFormOptions()
        {
            try
            {
                using TrafficSafetyDBContext db = new TrafficSafetyDBContext();

                List<string> vehicleTypeFilters = GetVehicleTypeFilters(GetComboBoxText(cmbVehicleType));

                List<string> violations = db.Set<TrafficLaw>()
                    .Where(l => vehicleTypeFilters.Count == 0 || vehicleTypeFilters.Any(filter => l.VehicleType.Contains(filter)))
                    .Select(l => l.ViolationDescription)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                if (violations.Count == 0)
                {
                    violations = db.Set<TrafficLaw>()
                        .Select(l => l.ViolationDescription)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct()
                        .OrderBy(v => v)
                        .ToList();
                }

                _availableTrafficLawViolations.Clear();
                _availableTrafficLawViolations.AddRange(violations);

                RefreshViolationSuggestions(string.Empty, false);

                if (_selectedViolations.Count > 0)
                {
                    _selectedViolations.RemoveAll(v => !_availableTrafficLawViolations.Contains(v));
                    RenderSelectedViolations();
                }

            }
            catch
            {
            }
        }

        private void InitializeViolationTypeSearch()
        {
            cmbViolationType.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(CmbViolationType_TextChanged));
            cmbViolationType.DropDownOpened += CmbViolationType_DropDownOpened;
        }

        private void CmbViolationType_DropDownOpened(object? sender, EventArgs e)
        {
            if (!_isUpdatingViolationSuggestions)
            {
                RefreshViolationSuggestions(cmbViolationType.Text, false);
            }
        }

        private void cmbVehicleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            _selectedViolations.Clear();
            RenderSelectedViolations();
            LoadFormOptions();
        }

        private void cmbViolationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingViolationSuggestions || cmbViolationType.SelectedItem is not ComboBoxItem item)
            {
                return;
            }

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
            if (_isUpdatingViolationSuggestions || e.OriginalSource is not TextBox)
            {
                return;
            }

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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _availableTrafficLawViolations.Take(50).ToList();
            }

            string normalizedKeyword = NormalizeSearchText(keyword);

            return _availableTrafficLawViolations
                .Select(item => new
                {
                    Value = item,
                    Score = GetSearchScore(item, normalizedKeyword)
                })
                .Where(item => item.Score < int.MaxValue)
                .OrderBy(item => item.Score)
                .ThenBy(item => item.Value)
                .Take(50)
                .Select(item => item.Value)
                .ToList();
        }

        private static int GetSearchScore(string source, string normalizedKeyword)
        {
            string normalizedSource = NormalizeSearchText(source);

            if (normalizedSource == normalizedKeyword)
            {
                return 0;
            }

            if (normalizedSource.StartsWith(normalizedKeyword, StringComparison.Ordinal))
            {
                return 10;
            }

            int containsIndex = normalizedSource.IndexOf(normalizedKeyword, StringComparison.Ordinal);
            if (containsIndex >= 0)
            {
                return 20 + containsIndex;
            }

            string[] words = normalizedSource.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int bestWordDistance = words.Length == 0
                ? int.MaxValue
                : words.Min(word => ComputeLevenshteinDistance(word, normalizedKeyword));

            int fullDistance = ComputeLevenshteinDistance(normalizedSource, normalizedKeyword);
            int bestDistance = Math.Min(fullDistance, bestWordDistance);

            return bestDistance <= Math.Max(2, normalizedKeyword.Length / 3)
                ? 100 + bestDistance
                : int.MaxValue;
        }

        private static string NormalizeSearchText(string text)
        {
            string decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder();

            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

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
            if (string.IsNullOrEmpty(source))
            {
                return target.Length;
            }

            if (string.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            int[,] matrix = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= target.Length; j++)
            {
                matrix[0, j] = j;
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
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
            if (string.IsNullOrWhiteSpace(currentText))
            {
                return;
            }

            string? match = _availableTrafficLawViolations
                .FirstOrDefault(v => string.Equals(v, currentText, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                AddSelectedViolation(match);
            }
        }

        private void AddSelectedViolation(string? violation)
        {
            if (string.IsNullOrWhiteSpace(violation))
            {
                return;
            }

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

                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

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
                    Content = "�",
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

        private void UploadEvidenceArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png",
                Title = "ChẨn Ẩnh vi phạm"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

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
                txtEvidenceFileName.Text = "+ T?i hình ảnh lên";
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
            if (Uri.TryCreate(evidenceImagePath, UriKind.Absolute, out Uri? absoluteUri))
            {
                return absoluteUri;
            }

            string fullPath = IOPath.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                evidenceImagePath.TrimStart('/', '\\').Replace('/', IOPath.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                return new Uri(fullPath, UriKind.Absolute);
            }

            return Uri.TryCreate(evidenceImagePath, UriKind.Relative, out Uri? relativeUri)
                ? relativeUri
                : null;
        }

        private static string GetComboBoxText(ComboBox comboBox)
        {
            if (!string.IsNullOrWhiteSpace(comboBox.Text))
            {
                return comboBox.Text.Trim();
            }

            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                return item.Content?.ToString()?.Trim() ?? string.Empty;
            }

            return comboBox.Text?.Trim() ?? string.Empty;
        }

        private static void SetComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (object item in comboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem
                    && string.Equals(comboBoxItem.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }

            comboBox.Items.Add(new ComboBoxItem { Content = value });
            comboBox.SelectedIndex = comboBox.Items.Count - 1;
        }

        private static string NormalizeVehicleType(string vehicleType)
        {
            return vehicleType switch
            {
                "Xe Ô tô" => "Ô tô",
                "Xe tải" => "Ô tô",
                _ => vehicleType
            };
        }

        private static List<string> GetVehicleTypeFilters(string vehicleType)
        {
            return vehicleType switch
            {
                "Xe Ô tô" => new List<string> { "Ô tô", "Xe Ô tô" },
                "Xe tải" => new List<string> { "Xe tải", "Ô tô" },
                "Xe máy" => new List<string> { "Xe máy" },
                _ => new List<string>()
            };
        }

        private static TrafficLaw? FindTrafficLawForVehicleType(TrafficSafetyDBContext db, string violation, string vehicleType)
        {
            List<string> vehicleTypeFilters = GetVehicleTypeFilters(vehicleType);

            TrafficLaw? law = db.Set<TrafficLaw>()
                .FirstOrDefault(l => l.ViolationDescription == violation && vehicleTypeFilters.Any(filter => l.VehicleType.Contains(filter)));

            return law ?? db.Set<TrafficLaw>().FirstOrDefault(l => l.ViolationDescription == violation);
        }

        private void LoadViolationDetail()
        {
            if (_recordId is null)
                return;

            var detail = ViolationLookupService.GetViolationDetail(_recordId.Value);
            if (detail == null)
            {
                MessageBox.Show("Không tìm thấy thông tin chi tiết vi phạm.");
                return;
            }

            SetComboBoxSelection(cmbVehicleType, detail.VehicleType == "Ô tô" ? "Xe Ô tô" : detail.VehicleType);
            txtLicensePlate.Text = detail.HeaderTitle.Replace("Phương tiện ", string.Empty);
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
    }
}






