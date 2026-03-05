using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; // Đảm bảo đúng Namespace để xóa lỗi CS0246

[Table("NOTIFICATIONS")] // Ánh xạ đúng với bảng NOTIFICATIONS trong SQL
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Khai báo ID tự động tăng trong SQL
    [Column("NOTIFICATION_ID")]
    public int NotificationId { get; set; }

    [Column("CCCD")]
    public string CCCD { get; set; } = string.Empty;

    [Column("CONTENT")]
    public string Content { get; set; } = string.Empty;

    [Column("NOTIFICATION_TYPE")]
    public string Type { get; set; } = string.Empty;

    [Column("IS_READ")]
    public bool IsRead { get; set; } = false; // Thuộc tính mới từ SQL

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property - Mối quan hệ với bảng User
    [ForeignKey("CCCD")]
    public virtual User User { get; set; }

    public Notification() { }

    public Notification(User user, string content, string type)
    {
        this.User = user;
        this.CCCD = user.Cccd;
        this.Content = content;
        this.Type = type;
        this.IsRead = false;
        this.CreatedAt = DateTime.Now;
    }

    public string Display()
    {
        string status = IsRead ? "[Đã đọc]" : "[Chưa đọc]";
        return $"{status} {CreatedAt:dd/MM} - {Content}";
    }
}