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
            int currentRoleInt = 0; // Chuyển Role sang số nguyên giống Model của Nhi

            // 1. CHUÔNG TỰ ĐỘNG NHẬN DIỆN NGƯỜI DÙNG
            if (loggedInUser is Customer cust)
            {
                currentId = cust.Cccd;
                currentRoleInt = 3; // Giả sử 3 là Customer
            }
            else if (loggedInUser is Officer off)
            {
                currentId = off.OfficerId;
                currentRoleInt = 2; // Giả sử 2 là Officer
            }
            else if (loggedInUser is Admin ad)
            {
                currentId = ad.Username;
                currentRoleInt = 1; // Giả sử 1 là Admin
            }

            // 2. KẾT NỐI DATABASE VÀ LỌC THÔNG BÁO CHO ĐÚNG NGƯỜI
            using (var db = new TrafficSafetyDBContext()) // Thay AppDbContext bằng tên DbContext của Nhi
            {
                // Lấy thông báo: 
                // Điều kiện 1: Đúng Role (TargetRole == currentRoleInt)
                // Điều kiện 2: Gửi đích danh (TargetId == currentId) HOẶC gửi chung cho cả Role đó (TargetId == null || TargetId == "")
                var query = db.Notifications
                    .Where(n => n.TargetRole == currentRoleInt &&
                                (string.IsNullOrEmpty(n.TargetId) || n.TargetId == currentId))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10) // Chỉ lấy 10 thông báo mới nhất cho nhẹ máy
                    .ToList();

                // Chuyển dữ liệu từ Database sang ViewModel để hiển thị lên Giao diện
                var notifications = new List<NotificationViewModel>();
                foreach (var item in query)
                {
                    notifications.Add(new NotificationViewModel
                    {
                        Content = item.Content,
                        // Format lại thời gian cho đẹp, ví dụ: "10:30 25/04/2026"
                        Date = item.CreatedAt.ToString("HH:mm dd/MM/yyyy"),
                        IsUnread = !item.IsRead
                    });
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
}