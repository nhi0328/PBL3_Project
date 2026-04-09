using PBL3.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PBL3
{
    public partial class Page21 : Page
    {
        // CHỈ NHẬN OFFICER
        private readonly Officer _currentUser;
        private readonly LuatItem _currentLuat;
        private readonly bool _isEditMode;

        // Constructor mặc định
        public Page21()
        {
            InitializeComponent();
        }

        // Constructor chính nhận dữ liệu
        public Page21(LuatItem luat, Officer user = null)
        {
            InitializeComponent();
            _currentUser = user;
            _currentLuat = luat;

            if (_currentUser != null)
            {
                txtUserName.Text = $"Cán bộ: {_currentUser.OfficerId}";
            }

            // NẾU CÓ TRUYỀN LUẬT SANG -> CHẾ ĐỘ CHỈNH SỬA
            if (_currentLuat != null)
            {
                _isEditMode = true;
                txtTieuDe.Text = _currentLuat.TenLoi;
                txtNghiDinh.Text = _currentLuat.CanCu;
                txtNgayBanHanh.Text = _currentLuat.NgayBanHanh;
                txtNgayHieuLuc.Text = _currentLuat.NgayHieuLuc;
                txtMucPhatXeMay.Text = _currentLuat.PhatTienXeMay;
                txtMucPhatOto.Text = _currentLuat.PhatTienOto;

                // Tách số từ chuỗi "Trừ X điểm"
                string truDiemNum = string.Join("", (_currentLuat.TruDiem ?? "").Where(char.IsDigit));
                txtTruDiem.Text = truDiemNum;

                UpdateNoiDung();
            }
            // NẾU KHÔNG CÓ -> CHẾ ĐỘ THÊM MỚI
            else
            {
                _isEditMode = false;
            }

            // Đăng ký sự kiện TextChanged để tự động cập nhật Nội dung
            txtMucPhatXeMay.TextChanged += (s, e) => UpdateNoiDung();
            txtMucPhatOto.TextChanged += (s, e) => UpdateNoiDung();
            txtTruDiem.TextChanged += (s, e) => UpdateNoiDung();
        }

        // --- CẬP NHẬT TEXTBLOCK XEM TRƯỚC NỘI DUNG ---
        private void UpdateNoiDung()
        {
            string noidung = "";
            if (!string.IsNullOrWhiteSpace(txtMucPhatXeMay.Text))
            {
                noidung += $"- Phạt tiền từ {txtMucPhatXeMay.Text} đồng đối với người điều khiển xe mô tô, xe máy\n";
            }
            if (!string.IsNullOrWhiteSpace(txtMucPhatOto.Text))
            {
                noidung += $"- Phạt tiền từ {txtMucPhatOto.Text} đồng đối với người điều khiển xe Ô tô\n";
            }
            if (!string.IsNullOrWhiteSpace(txtTruDiem.Text))
            {
                noidung += $"- Trừ {txtTruDiem.Text} điểm bằng lái xe";
            }
            txtNoiDung.Text = noidung.TrimEnd();
        }

        // --- NÚT LƯU LUẬT (DÙNG ENTITY FRAMEWORK) ---
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTieuDe.Text))
            {
                new CustomMessageBox("Vui lòng nhập Tiêu đề luật.", "Thông báo").ShowDialog();
                return;
            }

            try
            {
                using (var db = new TrafficSafetyDBContext())
                {
                    TrafficLaw lawToSave;

                    // 1. NẾU LÀ CHỈNH SỬA
                    if (_isEditMode && _currentLuat != null)
                    {
                        lawToSave = db.TrafficLaws.FirstOrDefault(l => l.LawId == _currentLuat.LawId);
                        if (lawToSave == null)
                        {
                            new CustomMessageBox("Không tìm thấy luật để cập nhật.", "Lỗi").ShowDialog();
                            return;
                        }

                        // Cập nhật tên luật
                        lawToSave.LawName = txtTieuDe.Text;

                        // Xóa sạch các chi tiết phạt cũ (Ô tô, Xe máy) để ghi đè cái mới
                        var oldDetails = db.TrafficLawDetails.Where(d => d.LawId == lawToSave.LawId);
                        db.TrafficLawDetails.RemoveRange(oldDetails);
                    }
                    // 2. NẾU LÀ THÊM MỚI
                    else
                    {
                        lawToSave = new TrafficLaw
                        {
                            LawName = txtTieuDe.Text
                        };
                        db.TrafficLaws.Add(lawToSave);
                    }

                    // Lưu thay đổi vào bảng TRAFFIC_LAWS trước để lấy LawId (Mã luật tự tăng)
                    db.SaveChanges();

                    // 3. THÊM CHI TIẾT MỨC PHẠT VÀO BẢNG TRAFFIC_LAW_DETAILS
                    if (!string.IsNullOrWhiteSpace(txtMucPhatXeMay.Text))
                    {
                        db.TrafficLawDetails.Add(CreateLawDetail(lawToSave.LawId, "Xe máy", txtMucPhatXeMay.Text));
                    }

                    if (!string.IsNullOrWhiteSpace(txtMucPhatOto.Text))
                    {
                        db.TrafficLawDetails.Add(CreateLawDetail(lawToSave.LawId, "Ô tô", txtMucPhatOto.Text));
                    }

                    // Lưu tất cả các thay đổi
                    db.SaveChanges();

                    new CustomMessageBox("Đã lưu thông tin luật thành công!", "Thông báo").ShowDialog();

                    // Trở về trang trước
                    if (NavigationService.CanGoBack) NavigationService.GoBack();
                    else NavigationService.Navigate(new Page13(_currentUser));
                }
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi lưu vào CSDL: " + ex.Message, "Lỗi").ShowDialog();
            }
        }

        // --- HÀM TẠO ĐỐI TƯỢNG CHI TIẾT LUẬT (Hàm này nãy Nhi bị rớt mất nè) ---
        private TrafficLawDetail CreateLawDetail(int lawId, string vehicleType, string fineAmount)
        {
            // Parse điểm trừ
            int demeritPoints = 0;
            if (int.TryParse(txtTruDiem.Text, out int points))
            {
                demeritPoints = points;
            }

            // Parse Ngày (An toàn)
            DateTime? issueDate = null;
            if (DateTime.TryParseExact(txtNgayBanHanh.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedIssue))
                issueDate = parsedIssue;

            DateTime? effectiveDate = null;
            if (DateTime.TryParseExact(txtNgayHieuLuc.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEff))
                effectiveDate = parsedEff;

            return new TrafficLawDetail
            {
                LawId = lawId,
                CategoryId = null,
                FineAmount = fineAmount,
                DemeritPoints = demeritPoints,
                Decree = txtNghiDinh.Text ?? string.Empty,
                IssueDate = issueDate,
                EffectiveDate = effectiveDate
            };
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG BÊN DƯỚI & SIDEBAR ---
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page13(_currentUser));
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page16(_currentUser));
        private void btnLogOut_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
    }
}