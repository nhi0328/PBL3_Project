using PBL3.Models;
using PBL3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PBL3
{
    public partial class Page32 : Page
    {
        private readonly Customer _currentUser;
        private DispatcherTimer _timer;
        private int _timeRemaining = 60;

        // Biến lưu OTP thực tế để so sánh
        private string _generatedOTP = string.Empty;

        // Thông tin Email Bot (Thay bằng thông tin của Nhi)
        private const string SenderEmail = "hethongcsgt@gmail.com";
        private const string SenderAppPassword = "vflrfsiyjdaqjeng";

        public Page32() { InitializeComponent(); }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page1());
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Page6()); // Trang thông tin cá nhân
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            // Mở Menu
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // Constructor nhận thông tin User
        public Page32(string tenNguoiDung) : this()
        {
            // Kiểm tra nếu có tên thì gán vào TextBlock
            if (!string.IsNullOrEmpty(tenNguoiDung))
            {
                txtUserName.Text = tenNguoiDung;
            }
        }

        public Page32(Customer user) : this()
        {
            _currentUser = user;
            this.Loaded += Page32_Loaded;
            if (_currentUser != null)
            {
                txtUserName.Text = _currentUser.FullName;

                myBell.LoadData(_currentUser as Customer);
            }
        }

        private void Page32_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Email))
            {
                runEmail.Text = MaskEmail(_currentUser.Email);
                SendOTPToEmail(_currentUser.Email);
                StartTimer();
            }
            else
            {
                new CustomMessageBox("Không tìm thấy Email của bạn để gửi mã xác nhận!", "Lỗi").ShowDialog();
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;

            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 5)
            {
                name = name.Substring(0, 1) + new string('*', name.Length - 1);
            }
            else
            {
                string firstPart = name.Substring(0, 3);
                string lastPart = name.Substring(name.Length - 2);
                string middle = new string('*', name.Length - 5);
                name = firstPart + middle + lastPart;
            }

            return name + "@" + domain + ".";
        }

        private void StartTimer()
        {
            _timeRemaining = 60;
            runTimer.Text = _timeRemaining + "s";
            btnGuiLaiMa.IsEnabled = false;
            btnGuiLaiMa.Foreground = new SolidColorBrush(Colors.LightGray);
            btnGuiLaiMa.Cursor = Cursors.Arrow;

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
            }
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeRemaining--;
            if (_timeRemaining > 0)
            {
                runTimer.Text = _timeRemaining + "s";
            }
            else
            {
                _timer.Stop();
                runTimer.Text = "0s";
                btnGuiLaiMa.IsEnabled = true;
                btnGuiLaiMa.Foreground = new SolidColorBrush(Colors.Black);
                btnGuiLaiMa.Cursor = Cursors.Hand;
            }
        }

        // --- HÀM TẠO VÀ GỬI OTP QUA EMAIL ---
        private void SendOTPToEmail(string recipientEmail)
        {
            try
            {
                // 1. Tạo ngẫu nhiên 6 chữ số
                Random rand = new Random();
                _generatedOTP = rand.Next(100000, 999999).ToString();

                // 2. Cấu hình Email
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(SenderEmail, "Hệ Thống CSGT");
                mail.To.Add(recipientEmail);
                mail.Subject = "[CẢNH BÁO BẢO MẬT] Mã xác thực (OTP) tài khoản";
                mail.IsBodyHtml = true;

                // "VĂN MẪU" TỰ ĐỘNG CỰC KỲ CHUYÊN NGHIỆP:
                string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>
                    <h2 style='color: #C62828; text-align: center; border-bottom: 2px solid #C62828; padding-bottom: 10px;'>
                        BỘ GIAO THÔNG VẬN TẢI
                    </h2>
                    <p style='font-size: 16px; color: #333;'>Chào bạn,</p>
                    <p style='font-size: 16px; color: #333;'>Bạn (hoặc ai đó) vừa yêu cầu một mã xác thực (OTP) để thực hiện thao tác bảo mật trên hệ thống <b>Tra cứu & Quản lý Vi phạm Giao thông</b>.</p>
        
                    <div style='background-color: #f9f9f9; padding: 20px; text-align: center; border-radius: 8px; margin: 25px 0; border: 1px dashed #C62828;'>
                        <p style='margin: 0; font-size: 16px; color: #555;'>Mã xác thực của bạn là:</p>
                        <span style='display: block; font-size: 36px; font-weight: bold; color: #C62828; letter-spacing: 8px; margin-top: 10px;'>
                            {_generatedOTP}
                        </span>
                    </div>
        
                    <p style='font-size: 15px; color: #333;'>Mã này chỉ có hiệu lực trong vòng <b>3 phút</b>.</p>
                    <p style='color: red; font-size: 14px; font-weight: bold;'>
                        ⚠️ LƯU Ý: Tuyệt đối không chia sẻ mã này cho bất kỳ ai để tránh rủi ro mất tài khoản.
                    </p>
        
                    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0;'>
                    <p style='font-size: 12px; color: #888; text-align: center; margin: 0;'>
                        Đây là email tự động từ hệ thống, vui lòng không trả lời email này.<br>
                        Trân trọng!
                    </p>
                </div>";

                // Gắn cái văn mẫu siêu đẹp đó vào thân mail
                mail.Body = emailBody;
                // 3. Cấu hình Server SMTP của Google
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(SenderEmail, SenderAppPassword);
                smtp.EnableSsl = true; // Bắt buộc phải bật SSL

                // 4. Gửi đi
                smtp.Send(mail);
                new CustomMessageBox($"Đã gửi mã xác nhận đến Email: {recipientEmail}", "Thông báo").ShowDialog();
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi gửi Email: " + ex.Message, "Lỗi hệ thống").ShowDialog();
            }
        }

        // --- XỬ LÝ NÚT GỬI LẠI MÃ ---
        private void btnGuiLaiMa_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Email))
            {
                SendOTPToEmail(_currentUser.Email);
                StartTimer();
            }
        }

        // --- XỬ LÝ NÚT XÁC NHẬN ---
        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu từ 6 ô TextBox ghép lại (Nhớ đặt tên x:Name bên XAML nhé)
            string userEnteredOTP = $"{txtOTP1.Text}{txtOTP2.Text}{txtOTP3.Text}{txtOTP4.Text}{txtOTP5.Text}{txtOTP6.Text}";

            if (string.IsNullOrWhiteSpace(_generatedOTP))
            {
                new CustomMessageBox("Mã OTP chưa được tạo. Vui lòng bấm 'Gửi lại mã'.", "Lỗi").ShowDialog();
                return;
            }

            if (userEnteredOTP == _generatedOTP)
            {
                new CustomMessageBox("Xác nhận thành công!", "Thành công").ShowDialog();
                NavigationService.Navigate(new Page33(_currentUser));
            }
            else
            {
                new CustomMessageBox("Mã xác nhận không đúng. Vui lòng kiểm tra lại!", "Thất bại").ShowDialog();
            }
        }

        private void btnQuayLai_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService.Navigate(new Page7(_currentUser as Customer));
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
    }
}
