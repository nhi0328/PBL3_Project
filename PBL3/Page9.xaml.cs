using Microsoft.IdentityModel.Tokens;
using PBL3.Models;
using System;
using System.Collections.Generic;
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

namespace PBL3
{
    public partial class Page9 : Page
    {
        private readonly Customer _currentUser;
        private readonly int _recordId;

        public Page9(Customer user, int recordId)
        {
            InitializeComponent();
            _currentUser = user;
            _recordId = recordId;

            // Hiển thị tên người dùng
            if (_currentUser != null && txtUserName != null)
            {
                txtUserName.Text = _currentUser.FullName;
                myBell.LoadData(_currentUser as Customer);
            }

            // Gọi hàm tải dữ liệu khi vừa mở trang
            LoadRecordDetail();
        }

        // Hàm lấy chi tiết biên bản và cập nhật toàn bộ giao diện
        private void LoadRecordDetail()
        {
            try
            {
                using var db = new TrafficSafetyDBContext();

                // 1. LẤY DỮ LIỆU BIÊN BẢN TỪ DATABASE
                var record = db.ViolationRecords.FirstOrDefault(r => r.ViolationRecordId == _recordId);

                if (record != null)
                {
                    // --- ĐỔ DỮ LIỆU VÀO GIAO DIỆN XAML ---

                    // Tiêu đề & Phụ đề
                    txtHeaderTitle.Text = "THÔNG TIN CHI TIẾT BIÊN BẢN";
                    txtHeaderSubtitle.Text = $"Biển số xe: {record.LicensePlate} - Mã BB: #{record.ViolationRecordId}";

                    // Các thông tin cơ bản
                    txtVehicleTypeValue.Text = record.Category != null && !string.IsNullOrEmpty(record.Category.CategoryName) ? record.Category.CategoryName : "Không rõ";

                    // Format Ngày và Giờ (Kèm check null để không bị crash)
                    txtViolationDateValue.Text = record.ViolationDate != null
                        ? record.ViolationDate.Value.ToString("dd/MM/yyyy")
                        : "Đang cập nhật";

                    txtViolationTimeValue.Text = record.ViolationTime != null
                        ? record.ViolationTime.ToString() // Nếu là TimeSpan thì gọi ToString()
                        : "Đang cập nhật";

                    txtViolationLocationValue.Text = string.IsNullOrEmpty(record.Address) ? "Không rõ" : record.Address;
                    txtViolationDescriptionValue.Text = string.IsNullOrEmpty(record.ViolationDetail) ? "Không rõ" : record.ViolationDetail;

                    // Truy xuất thêm thông tin Luật để lấy Mức phạt (nếu có)
                    var law = db.TrafficLaws.FirstOrDefault(l => l.LawId == record.LawId);
                    if (law != null)
                    {
                        // Giả sử bảng Luật của Nhi có cột mô tả mức phạt, hoặc lấy tên luật hiển thị tạm
                        txtFineRangeValue.Text = "Theo quy định tại " + law.LawName;
                    }
                    else
                    {
                        txtFineRangeValue.Text = "Chờ quyết định xử phạt";
                    }

                    txtPaymentLocationValue.Text = "Kho bạc Nhà nước / Cổng DVC Quốc gia"; // Địa điểm mặc định

                    // Xử lý Trạng thái và Tô màu tự động
                    string status = record.StatusText;
                    txtStatusValue.Text = status;

                    // Tô màu: Chưa xử lý -> Đỏ, Đã xử lý/Đóng phạt -> Xanh lá
                    if (status.ToLower().Contains("chưa") || status.ToLower().Contains("vi phạm"))
                    {
                        txtStatusValue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")); // Đỏ
                    }
                    else
                    {
                        txtStatusValue.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")); // Xanh lá
                    }

                    // --- XỬ LÝ HÌNH ẢNH VI PHẠM ---
                    if (!string.IsNullOrEmpty(record.ImagePath))
                    {
                        try
                        {
                            // Tải ảnh từ đường dẫn
                            imgEvidence.Source = new BitmapImage(new Uri(record.ImagePath, UriKind.RelativeOrAbsolute));
                            imgEvidence.Visibility = Visibility.Visible;
                            txtEvidencePlaceholder.Visibility = Visibility.Collapsed; // Ẩn chữ "Chưa có hình"
                            txtEvidenceCaption.Text = "Hình ảnh trích xuất từ camera/thiết bị nghiệp vụ";
                        }
                        catch
                        {
                            // Nếu đường dẫn ảnh bị lỗi hoặc không tìm thấy file
                            txtEvidencePlaceholder.Text = "Lỗi không tải được ảnh";
                            imgEvidence.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        txtEvidencePlaceholder.Text = "Chưa có hình minh họa";
                        imgEvidence.Visibility = Visibility.Collapsed;
                    }

                    // --- 2. QUÉT BẢNG SYSTEM_LOGS ĐỂ TÌM THỜI GIAN LAST UPDATE ---
                    // Biên bản (Violation Record) dùng Prefix là "B"
                    var lastLog = db.SystemLogs
                                    .Where(log => log.TargetPrefix == "B" && log.TargetValue == _recordId.ToString())
                                    .OrderByDescending(log => log.Time)
                                    .FirstOrDefault();

                    DateTime lastUpdate;

                    if (lastLog != null)
                    {
                        // Lấy thời gian ghi nhận từ hệ thống Log
                        lastUpdate = lastLog.Time;
                    }
                    else
                    {
                        // Nếu chưa từng có ai cập nhật, lấy Ngày vi phạm làm ngày tạo mặc định
                        lastUpdate = record.ViolationDate ?? DateTime.Now;
                    }

                    // Hiển thị thời gian cập nhật lên màn hình
                    txtLastUpdatedValue.Text = lastUpdate.ToString("HH:mm - dd/MM/yyyy");
                }
                else
                {
                    MessageBox.Show("Dữ liệu biên bản này không tồn tại hoặc đã bị xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Có thể tự động quay lại trang trước nếu không tìm thấy
                    if (NavigationService?.CanGoBack == true)
                    {
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }
       
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
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


        //Chuyển qua trang Tra cứu nhanh
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page4(_currentUser as Customer));
        }

        // Chuyển trang Tra cứu luật
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page5(_currentUser as Customer));
        }

        // Chuyển trang Quản lý phương tiện
        private void btnQLPT_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6(_currentUser as Customer));
        }

        //Chuyển trang Quản lý tài khoản
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page7(_currentUser as Customer));
        }

        // chuyển trang Phản ánh
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page8(_currentUser as Customer));
        }

        // Đăng xuất
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }
    }
}
