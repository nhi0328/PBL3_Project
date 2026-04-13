using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3
{
    public class BarData
    {
        public double Height { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class ChartColumn
    {
        public List<BarData> Series { get; set; }
    }

    public class LegendItem
    {
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public partial class Page49 : Page
    {
        private readonly Admin _currentUser;
        private List<Category> _categories;
        private List<SolidColorBrush> _colors = new List<SolidColorBrush>
        {
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4472C4")), // Xe máy
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED7D31")), // Xe máy điện
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A5A5A5")), // Ô tô
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC000")), 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B9BD5")),
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70AD47")),
        };

        public Page49()
        {
            InitializeComponent();
            this.Loaded += Page49_Loaded;
        }

        public Page49(Admin user) : this()
        {
            _currentUser = user;
            if (_currentUser != null)
            {
                txtUserName.Text = $"Quản trị viên";
                if(myBell != null) myBell.LoadData(_currentUser as Admin);
            }
        }

        private void Page49_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            LoadCategories();
            cboType.SelectedIndex = 0;
        }

        private void LoadCategories()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();
                _categories = db.Categories.ToList();
            }
            catch (Exception)
            {
                _categories = new List<Category>();
            }
        }

        private void cboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboValue == null || cboType == null) return;
            cboValue.Items.Clear();
            var index = cboType.SelectedIndex;
            int currentYear = DateTime.Now.Year;

            if (index == 0) // Theo năm
            {
                int startYear = 2015;
                while (startYear <= currentYear)
                {
                    int endYear = startYear + 4;
                    if (startYear == 2024 && currentYear >= 2024)
                        cboValue.Items.Add($"{startYear}-năm hiện tại"); // or similar
                    else if (endYear >= currentYear)
                        cboValue.Items.Add($"{startYear}-{currentYear}");
                    else
                        cboValue.Items.Add($"{startYear}-{endYear}");
                    
                    if (startYear == 2024) break; // Because prompt specific "2024-năm hiện tại"
                    startYear += 5;
                    // Fix exact prompt wording: "2015-2019, 2020-2024, 2024-năm hiện tại"
                }
                // Overriding loop to match exact prompt wording:
                cboValue.Items.Clear();
                cboValue.Items.Add("2015-2019");
                cboValue.Items.Add("2020-2024");
                if (currentYear >= 2024) cboValue.Items.Add($"2024-{currentYear}");
            }
            else if (index == 1) // Theo quý
            {
                for (int y = 2015; y <= currentYear; y++)
                {
                    cboValue.Items.Add(y.ToString());
                }
            }
            else if (index == 2) // Theo tháng
            {
                // "khung chọn thứ 2 sẽ hiển thị các quý của năm hiện tại cho đến hiện tại"
                int maxQ = (DateTime.Now.Month - 1) / 3 + 1;
                for (int q = 1; q <= maxQ; q++)
                {
                    cboValue.Items.Add($"Quý {q} - {currentYear}");
                }
            }
            else if (index == 3) // Theo tuần
            {
                // "các tháng của quý hiện tại"
                int currentQ = (DateTime.Now.Month - 1) / 3 + 1;
                int mStart = (currentQ - 1) * 3 + 1;
                int mEnd = Math.Min(mStart + 2, DateTime.Now.Month);
                for (int m = mStart; m <= mEnd; m++)
                {
                     cboValue.Items.Add($"Tháng {m} - {currentYear}");
                }
            }
            else if (index == 4) // Theo ngày
            {
                // "các tuần của tháng hiện tại"
                int m = DateTime.Now.Month;
                int y = DateTime.Now.Year;
                DateTime firstDay = new DateTime(y, m, 1);
                DateTime current = firstDay;
                int weekCount = 1;

                while (current.Month == m && current <= DateTime.Now)
                {
                    // Find Monday
                    int diff = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime monday = current.AddDays(-1 * diff).Date;
                    DateTime sunday = monday.AddDays(6).Date;

                    string weekStr = $"Tuần {weekCount} ({monday:dd/MM/yyyy}-{sunday:dd/MM/yyyy})";
                    cboValue.Items.Add(weekStr);

                    current = sunday.AddDays(1);
                    weekCount++;
                }
            }

            if (cboValue.Items.Count > 0)
                cboValue.SelectedIndex = cboValue.Items.Count - 1;
        }

        private void cboValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChartData();
        }

        private void tgPhanLoai_Checked(object sender, RoutedEventArgs e)
        {
            UpdateChartData();
        }

        private void UpdateChartData()
        {
            if (cboValue == null || cboValue.SelectedItem == null || cboType == null) return;
            
            int index = cboType.SelectedIndex;
            string val = cboValue.SelectedItem.ToString();
            bool phanLoai = tgPhanLoai.IsChecked == true;

            using var db = new TrafficSafetyDBContext();
            var allRecords = db.ViolationRecords.Where(r => r.ViolationDate != null).ToList();

            List<string> xAxisLabels = new List<string>();
            List<ViolationRecord> currentData = new List<ViolationRecord>();
            List<ViolationRecord> previousData = new List<ViolationRecord>();

            DateTime now = DateTime.Now;

            // Xây dựng List chứa thông tin filter để lấy List<List<ViolationRecord>> tương ứng vs các nhãn X
            List<List<ViolationRecord>> columnsData = new List<List<ViolationRecord>>();
            string timeUnitName = "";

            if (index == 0) // Theo năm
            {
                string[] parts = val.Split('-');
                int startY = int.Parse(parts[0]);
                int endY = parts[1].Contains("hiện tại") ? now.Year : (int.TryParse(parts[1], out int pY) ? pY : now.Year);
                if (endY - startY > 4) endY = startY + 4; // limit 5
                
                txtChartTitle.Text = $"Biểu đồ thống kê lỗi vi phạm khoảng từ {startY} đến {endY}";
                timeUnitName = "cụm năm";

                for (int y = startY; y <= endY; y++)
                {
                    xAxisLabels.Add(y.ToString());
                    var colRecs = allRecords.Where(r => r.ViolationDate.Value.Year == y).ToList();
                    columnsData.Add(colRecs);
                    currentData.AddRange(colRecs);
                }

                int prevStartY = startY - 5;
                int prevEndY = startY - 1;
                previousData = allRecords.Where(r => r.ViolationDate.Value.Year >= prevStartY && r.ViolationDate.Value.Year <= prevEndY).ToList();
            }
            else if (index == 1) // Theo quý
            {
                int year = int.Parse(val);
                txtChartTitle.Text = $"Biểu đồ thống kê lỗi vi phạm theo quý năm {year}";
                timeUnitName = "năm";

                for (int q = 1; q <= 4; q++)
                {
                    xAxisLabels.Add($"Quý {q}");
                    var colRecs = allRecords.Where(r => r.ViolationDate.Value.Year == year && (r.ViolationDate.Value.Month - 1) / 3 + 1 == q).ToList();
                    columnsData.Add(colRecs);
                    currentData.AddRange(colRecs);
                }
                previousData = allRecords.Where(r => r.ViolationDate.Value.Year == year - 1).ToList();
            }
            else if (index == 2) // Theo tháng
            {
                // val = "Quý X - YYYY"
                string[] p = val.Split(' ');
                int q = int.Parse(p[1]);
                int year = int.Parse(p[3]);

                txtChartTitle.Text = $"Biểu đồ thống kê lỗi vi phạm theo tháng (Quý {q}/{year})";
                timeUnitName = "quý";

                int startM = (q - 1) * 3 + 1;
                for (int m = startM; m <= startM + 2; m++)
                {
                    xAxisLabels.Add($"Tháng {m}");
                    var colRecs = allRecords.Where(r => r.ViolationDate.Value.Year == year && r.ViolationDate.Value.Month == m).ToList();
                    columnsData.Add(colRecs);
                    currentData.AddRange(colRecs);
                }

                // Prev Quý
                int prevQ = q == 1 ? 4 : q - 1;
                int prevY = q == 1 ? year - 1 : year;
                int pStartM = (prevQ - 1) * 3 + 1;
                int pEndM = pStartM + 2;
                previousData = allRecords.Where(r => r.ViolationDate.Value.Year == prevY && r.ViolationDate.Value.Month >= pStartM && r.ViolationDate.Value.Month <= pEndM).ToList();
            }
            else if (index == 3) // Theo tuần
            {
                // val = "Tháng X - YYYY"
                string[] p = val.Split(' ');
                int month = int.Parse(p[1]);
                int year = int.Parse(p[3]);

                txtChartTitle.Text = $"Biểu đồ thống kê lỗi vi phạm các tuần trong tháng {month}/{year}";
                timeUnitName = "tháng";

                DateTime start = new DateTime(year, month, 1);
                
                DateTime it = start;
                int w = 1;
                while (it.Month == month)
                {
                    int diff = (7 + (it.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime mon = it.AddDays(-1 * diff).Date;
                    DateTime sun = mon.AddDays(6).Date;

                    xAxisLabels.Add($"Tuần {w}");
                    var colRecs = allRecords.Where(r => r.ViolationDate.Value.Date >= mon && r.ViolationDate.Value.Date <= sun).ToList();
                    columnsData.Add(colRecs);
                    currentData.AddRange(colRecs);

                    it = sun.AddDays(1);
                    w++;
                }

                // Prev month
                int pm = month == 1 ? 12 : month - 1;
                int py = month == 1 ? year - 1 : year;
                previousData = allRecords.Where(r => r.ViolationDate.Value.Year == py && r.ViolationDate.Value.Month == pm).ToList();
            }
            else if (index == 4) // Theo ngày
            {
                // val = "Tuần 1 (12/12/2024-18/12/2024)"
                int splitIdx = val.IndexOf('(');
                string dates = val.Substring(splitIdx + 1).TrimEnd(')');
                string[] dp = dates.Split('-');
                DateTime dMon = DateTime.ParseExact(dp[0], "dd/MM/yyyy", null);
                DateTime dSun = DateTime.ParseExact(dp[1], "dd/MM/yyyy", null);

                txtChartTitle.Text = $"Biểu đồ thống kê lỗi vi phạm theo ngày ({dMon:dd/MM} - {dSun:dd/MM})";
                timeUnitName = "tuần";

                for (DateTime d = dMon; d <= dSun; d = d.AddDays(1))
                {
                    string name = "";
                    switch (d.DayOfWeek)
                    {
                        case DayOfWeek.Monday: name = "Thứ 2"; break;
                        case DayOfWeek.Tuesday: name = "Thứ 3"; break;
                        case DayOfWeek.Wednesday: name = "Thứ 4"; break;
                        case DayOfWeek.Thursday: name = "Thứ 5"; break;
                        case DayOfWeek.Friday: name = "Thứ 6"; break;
                        case DayOfWeek.Saturday: name = "Thứ 7"; break;
                        case DayOfWeek.Sunday: name = "CN"; break;
                    }
                    xAxisLabels.Add($"{name}\n{d:dd/MM}");
                    
                    var colRecs = allRecords.Where(r => r.ViolationDate.Value.Date == d.Date).ToList();
                    columnsData.Add(colRecs);
                    currentData.AddRange(colRecs);
                }

                DateTime pdMon = dMon.AddDays(-7);
                DateTime pdSun = dSun.AddDays(-7);
                previousData = allRecords.Where(r => r.ViolationDate.Value.Date >= pdMon && r.ViolationDate.Value.Date <= pdSun).ToList();
            }

            // Calc Summary
            int curTotal = currentData.Count;
            int curUnprocessed = currentData.Count(r => r.Status == 0); // assuming Status 0=chưa xử lý
            int curProcessed = curTotal - curUnprocessed;

            int prevTotal = previousData.Count;
            int prevUnprocessed = previousData.Count(r => r.Status == 0);
            int prevProcessed = prevTotal - prevUnprocessed;

            txtTotalLabel.Text = curTotal.ToString();
            txtUnprocessedLabel.Text = curUnprocessed.ToString();
            txtProcessedLabel.Text = curProcessed.ToString();

            SetPercent(txtTotalPercent, curTotal, prevTotal, timeUnitName);
            SetPercent(txtUnprocessedPercent, curUnprocessed, prevUnprocessed, timeUnitName);
            SetPercent(txtProcessedPercent, curProcessed, prevProcessed, timeUnitName);

            // Draw chart!
            DrawChart(columnsData, xAxisLabels, phanLoai);
        }

        private void SetPercent(TextBlock block, int cur, int prev, string timeUnit)
        {
            if (prev == 0)
            {
                if (cur == 0) block.Text = $"Không đổi so với {timeUnit} trước";
                else block.Text = $"Tăng 100% so với {timeUnit} trước";
            }
            else
            {
                double percent = (double)(cur - prev) / prev * 100.0;
                string dir = percent > 0 ? "Tăng" : (percent < 0 ? "Giảm" : "Không đổi");
                block.Text = $"{dir} {Math.Abs(percent):F2}% so với {timeUnit} trước";
            }
        }

        private void DrawChart(List<List<ViolationRecord>> cols, List<string> xLabels, bool isPhanLoai)
        {
            if (_categories == null || _categories.Count == 0) return;

            // Max value logic
            double maxValue = 0;
            
            List<ChartColumn> barsData = new List<ChartColumn>();

            foreach (var col in cols)
            {
                var barColl = new ChartColumn { Series = new List<BarData>() };

                if (isPhanLoai)
                {
                    for (int i = 0; i < _categories.Count; i++)
                    {
                        var cat = _categories[i];
                        int v = col.Count(r => r.CategoryId == cat.CategoryId);
                        if (v > maxValue) maxValue = v;

                        barColl.Series.Add(new BarData 
                        {
                            Color = _colors[i % _colors.Count],
                            Height = v
                        });
                    }
                }
                else
                {
                    double v = col.Count;
                    if (v > maxValue) maxValue = v;
                    barColl.Series.Add(new BarData 
                    {
                        Color = _colors[0],
                        Height = v
                    });
                }

                barsData.Add(barColl);
            }

            if (maxValue == 0) maxValue = 10;
            else if (maxValue < 10) maxValue += 5;
            else maxValue = maxValue * 1.2; 

            // Create Y axis labels and grid lines
            List<string> yLabels = new List<string>();
            List<object> yLines = new List<object>();

            int div = 5;
            double step = maxValue / div;

            for (int i = 1; i <= div; i++)
            {
                yLabels.Insert(0, ((int)(step * i)).ToString());
                yLines.Add(new object());
            }

            icYAxisLabels.ItemsSource = yLabels;
            icYAxisLines.ItemsSource = yLines;

            // Scale bars Data
            double chartHeight = 300; // Expected chart wrapper height
            foreach (var col in barsData) 
            {
                foreach (var s in col.Series)
                {
                    s.Height = (s.Height / maxValue) * chartHeight;
                    if (s.Height < 0) s.Height = 0;
                }
            }

            icBars.ItemsSource = barsData;
            icXAxisLabels.ItemsSource = xLabels;

            // Legends
            List<LegendItem> legends = new List<LegendItem>();
            if (isPhanLoai)
            {
                for (int i = 0; i < _categories.Count; i++)
                {
                    legends.Add(new LegendItem { Name = _categories[i].CategoryName, Color = _colors[i % _colors.Count] });
                }
            }
            else
            {
                legends.Add(new LegendItem { Name = "Tổng vi phạm", Color = _colors[0] });
            }
            icLegend.ItemsSource = legends;
        }

        // Navigation code blocks
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
}


