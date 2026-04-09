using System;
using System.Collections.Generic;
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
using PBL3.Models;
using System.Windows.Threading;
using System.Net.Mail;
using System.Net;

namespace PBL3
{
    public partial class Page37 : Page
    {
        private DispatcherTimer _timer;
        private int _timeRemaining = 60;
        private string _generatedOTP = string.Empty;

        // Thay bằng thông tin của Nhi
        private const string SenderEmail = "hethongcsgt@gmail.com";
        private const string SenderAppPassword = "vflrfsiyjdaqjeng";

        public Page37()
        {
            InitializeComponent();
            this.Loaded += Page37_Loaded;
        }

        private void Page37_Loaded(object sender, RoutedEventArgs e)
        {
            // Dummy email for forget password UI - this could match passed data from Page 36
            string email = "anh*****06@gmail.com";
            runEmail.Text = email;
            SendOTPToEmail("anhthu06@gmail.com"); // Hardcoded or passed from page 36
            StartTimer();
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

        private void SendOTPToEmail(string recipientEmail)
        {
            try
            {
                Random rand = new Random();
                _generatedOTP = rand.Next(100000, 999999).ToString();

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(SenderEmail, "Hệ Thống CSGT");
                mail.To.Add(recipientEmail);
                mail.Subject = "[CẢNH BÁO BẢO MẬT] Mã xác thực (OTP) lấy lại mật khẩu";
                mail.IsBodyHtml = true;

                string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>
                    <h2 style='color: #C62828; text-align: center; border-bottom: 2px solid #C62828; padding-bottom: 10px;'>
                        BỘ GIAO THÔNG VẬN TẢI
                    </h2>
                    <p style='font-size: 16px; color: #333;'>Chào bạn,</p>
                    <p style='font-size: 16px; color: #333;'>Bạn đang yêu cầu lấy lại mật khẩu trên hệ thống <b>Tra cứu & Quản lý Vi phạm Giao thông</b>.</p>

                    <div style='background-color: #f9f9f9; padding: 20px; text-align: center; border-radius: 8px; margin: 25px 0; border: 1px dashed #C62828;'>
                        <p style='margin: 0; font-size: 16px; color: #555;'>Mã xác thực của bạn là:</p>
                        <span style='display: block; font-size: 36px; font-weight: bold; color: #C62828; letter-spacing: 8px; margin-top: 10px;'>
                            {_generatedOTP}
                        </span>
                    </div>

                    <p style='font-size: 15px; color: #333;'>Mã này chỉ có hiệu lực trong vòng <b>3 phút</b>.</p>
                </div>";

                mail.Body = emailBody;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(SenderEmail, SenderAppPassword);
                smtp.EnableSsl = true;

                // Để demo chạy không gửi mail thật lúc dev, comment smtp.Send nếu cần
                // smtp.Send(mail); // Uncomment to send
                _generatedOTP = "123456"; // Dummy for testing
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Lỗi khi gửi Email: " + ex.Message, "Lỗi hệ thống").ShowDialog();
            }
        }

        private void btnGuiLaiMa_Click(object sender, RoutedEventArgs e)
        {
            SendOTPToEmail("anhthu06@gmail.com");
            StartTimer();
            new CustomMessageBox("Đã gửi lại mã OTP", "Thông báo").ShowDialog();
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            string userEnteredOTP = $"{txtOTP1.Text}{txtOTP2.Text}{txtOTP3.Text}{txtOTP4.Text}{txtOTP5.Text}{txtOTP6.Text}";

            if (userEnteredOTP == _generatedOTP)
            {
                new CustomMessageBox("Xác nhận OTP thành công! Chuyển phần thiết lập lại MK.", "Thành công").ShowDialog();
                NavigationService.Navigate(new Page38()); // Chuyển trang đặt lại MK
            }
            else
            {
                new CustomMessageBox("Mã xác nhận không đúng. Vui lòng kiểm tra lại!", "Thất bại").ShowDialog();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new Page1());
        }
    }
}