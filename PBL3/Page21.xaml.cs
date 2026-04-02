using System;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using PBL3.Models;

namespace PBL3
{
    public partial class Page21 : Page
    {
        private User _currentUser;
        private LuatItem _currentLuat;
        private bool _isEditMode;
        private string _originalTenLoi;

        public Page21()
        {
            InitializeComponent();
        }

        public Page21(LuatItem luat, User user = null)
        {
            InitializeComponent();
            _currentUser = user;
            _currentLuat = luat;

            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;
            }

            if (_currentLuat != null)
            {
                _isEditMode = true;
                _originalTenLoi = _currentLuat.TenLoi;
                txtTieuDe.Text = _currentLuat.TenLoi;
                txtNghiDinh.Text = _currentLuat.CanCu;
                txtNgayBanHanh.Text = _currentLuat.NgayBanHanh;
                txtNgayHieuLuc.Text = _currentLuat.NgayHieuLuc;
                txtMucPhatXeMay.Text = _currentLuat.PhatTienXeMay;
                txtMucPhatOto.Text = _currentLuat.PhatTienOto;

                string truDiemNum = "";
                if (!string.IsNullOrEmpty(_currentLuat.TruDiem))
                {
                    var splitted = _currentLuat.TruDiem.Split(' ');
                    if (splitted.Length > 1) { truDiemNum = splitted[1]; }
                    else { truDiemNum = _currentLuat.TruDiem; }
                }
                txtTruDiem.Text = truDiemNum;

                UpdateNoiDung();
            }
            else
            {
                _isEditMode = false;
            }

            txtMucPhatXeMay.TextChanged += (s, e) => UpdateNoiDung();
            txtMucPhatOto.TextChanged += (s, e) => UpdateNoiDung();
            txtTruDiem.TextChanged += (s, e) => UpdateNoiDung();
        }

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

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTieuDe.Text))
            {
                MessageBox.Show("Vui lòng nh?p Tiêu đề luật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            if (_isEditMode)
                            {
                                // Xoá luật cũ (chi ti?t theo tên luật)
                                string delQuery = @"
                                    DELETE FROM TRAFFIC_LAW_DETAILS WHERE LAW_ID IN (SELECT LAW_ID FROM TRAFFIC_LAWS WHERE LAW_NAME = @OldTenLoi);
                                    DELETE FROM TRAFFIC_LAWS WHERE LAW_NAME = @OldTenLoi;";
                                using (SqlCommand cmdDel = new SqlCommand(delQuery, conn, transaction))
                                {
                                    cmdDel.Parameters.AddWithValue("@OldTenLoi", _originalTenLoi);
                                    cmdDel.ExecuteNonQuery();
                                }
                            }

                            // Thêm mới Xe máy
                            if (!string.IsNullOrWhiteSpace(txtMucPhatXeMay.Text))
                            {
                                InsertLaw(conn, transaction, "Xe máy", txtMucPhatXeMay.Text);
                            }

                            // Thêm mới Ô tô
                            if (!string.IsNullOrWhiteSpace(txtMucPhatOto.Text))
                            {
                                InsertLaw(conn, transaction, "Ô tô", txtMucPhatOto.Text);
                            }

                            transaction.Commit();
                            MessageBox.Show("Đã lưu thông tin luật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (NavigationService.CanGoBack)
                            {
                                NavigationService.GoBack();
                            }
                            else
                            {
                                NavigationService.Navigate(new Page13(_currentUser));
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi khi lưu vào CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi k?t n?i CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertLaw(SqlConnection conn, SqlTransaction transaction, string vehicleType, string fineRange)
        {
            string categoryId = vehicleType == "Xe máy" ? "2" : "1";

            string queryLaw = @"INSERT INTO TRAFFIC_LAWS (LAW_NAME) OUTPUT INSERTED.LAW_ID VALUES (@Desc)";
            int newLawId;
            using (SqlCommand cmd = new SqlCommand(queryLaw, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Desc", txtTieuDe.Text);
                newLawId = (int)cmd.ExecuteScalar();
            }

            string queryDetail = @"INSERT INTO TRAFFIC_LAW_DETAILS 
                            (LAW_ID, VEHICLE_TYPE, FINE_AMOUNT, DEMERIT_POINTS, DECREE, ISSUE_DATE, EFFECTIVE_DATE) 
                            VALUES (@LawId, @Type, @Fine, @Points, @Ref, @Issue, @Effective)";

            using (SqlCommand cmd = new SqlCommand(queryDetail, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LawId", newLawId);
                cmd.Parameters.AddWithValue("@Type", categoryId);
                cmd.Parameters.AddWithValue("@Fine", fineRange);

                string points = string.IsNullOrWhiteSpace(txtTruDiem.Text) ? "Trừ 0 điểm" : $"Trừ {txtTruDiem.Text} điểm";
                cmd.Parameters.AddWithValue("@Points", points);

                cmd.Parameters.AddWithValue("@Ref", txtNghiDinh.Text ?? string.Empty);

                if (DateTime.TryParseExact(txtNgayBanHanh.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime issueDate))
                    cmd.Parameters.AddWithValue("@Issue", issueDate);
                else
                    cmd.Parameters.AddWithValue("@Issue", DBNull.Value);

                if (DateTime.TryParseExact(txtNgayHieuLuc.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime effectiveDate))
                    cmd.Parameters.AddWithValue("@Effective", effectiveDate);
                else
                    cmd.Parameters.AddWithValue("@Effective", DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page13(_currentUser));
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

        private void MenuInfo_Click(object sender, RoutedEventArgs e) { }
        private void MenuAdminUI_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page9());
        private void MenuOfficerUI_Click(object sender, RoutedEventArgs e) { }
        private void MenuLogout_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
        private void btnTraCuuNhanh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page12(_currentUser));
        private void btnTraCuuLuat_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page13(_currentUser));
        private void btnLBBVP_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page14(_currentUser));
        private void btnTaiKhoan_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page15(_currentUser));
        private void btnPhanAnh_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page16(_currentUser));
        private void btnLogOut_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new Page1());
    }
}

