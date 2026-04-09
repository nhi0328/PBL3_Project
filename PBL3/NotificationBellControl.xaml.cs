using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PBL3.Models;
using PBL3.ViewModels;

namespace PBL3
{
    public partial class NotificationBellControl : UserControl
    {
        public NotificationBellControl()
        {
            InitializeComponent();
        }

        // Đổi tham số từ (Customer cust) thành (object loggedInUser)
        public void LoadData(object loggedInUser)
        {
            if (loggedInUser == null) return;

            string currentId = "";
            string currentRole = "";

            // 1. CHUÔNG TỰ ĐỘNG NHẬN DIỆN NGƯỜI DÙNG
            if (loggedInUser is Customer cust)
            {
                currentId = cust.Cccd;
                currentRole = "Customer";
            }
            else if (loggedInUser is Officer off)
            {
                currentId = off.OfficerId; // Giả sử model Officer có cột này
                currentRole = "Officer";
            }
            else if (loggedInUser is Admin ad)
            {
                currentId = ad.Username; // Giả sử model Admin có cột này
                currentRole = "Admin";
            }

            // 2. KẾT NỐI DATABASE VÀ LỌC THÔNG BÁO CHO ĐÚNG NGƯỜI
            // Nhi mở comment đoạn này ra khi ráp nối DB thật nhé:
            /*
            using (var db = new TrafficSafetyDBContext())
            {
                var logs = db.SystemLogs
                             .Where(x => x.ActorId == currentId && x.Role == currentRole)
                             .OrderByDescending(x => x.CreatedAt)
                             .Take(10) // Lấy 10 tin mới nhất
                             .ToList();
                             
                // Chuyển dữ liệu từ SystemLogs sang NotificationViewModel để hiển thị...
            }
            */

            // --- DỮ LIỆU TEST TẠM THỜI (Tùy vai trò mà hiện thông báo khác nhau) ---
            var notifications = new List<NotificationViewModel>();

            if (currentRole == "Admin")
            {
                notifications.Add(new NotificationViewModel { Content = "Cán bộ CSGT-01 vừa xóa 1 biên bản.", Date = "Hôm nay", IsUnread = true });
                notifications.Add(new NotificationViewModel { Content = "Có 5 phản ánh mới từ người dân chờ duyệt.", Date = "Hôm qua", IsUnread = true });
            }
            else if (currentRole == "Officer")
            {
                notifications.Add(new NotificationViewModel { Content = "Admin vừa cấp quyền truy cập hệ thống camera Quận 1 cho bạn.", Date = "Hôm nay", IsUnread = true });
            }
            else if (currentRole == "Customer")
            {
                notifications.Add(new NotificationViewModel { Content = "Phương tiện của bạn vừa có lỗi vi phạm mới.", Date = "Hôm nay", IsUnread = true });
            }

            // 3. ĐỔ DỮ LIỆU LÊN GIAO DIỆN CHUÔNG
            icNotifications.ItemsSource = notifications;

            int unreadCount = notifications.Count(n => n.IsUnread);
            if (unreadCount > 0)
            {
                txtUnreadCount.Text = unreadCount.ToString();
                txtUnreadCount.Parent.SetValue(VisibilityProperty, Visibility.Visible); // Hiện chấm đỏ
            }
            else
            {
                txtUnreadCount.Parent.SetValue(VisibilityProperty, Visibility.Collapsed); // Ẩn chấm đỏ
            }
        }
    }
}