using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    [Table("NOTIFICATIONS")]
    public class Notification
    {
        [Key]
        [Column("NOTIFICATION_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Column("TARGET_ROLE")]
        public int TargetRole { get; set; }

        [Column("TARGET_ID")]
        [StringLength(50)]
        public string TargetId { get; set; } // Có thể null

        [Column("CONTENT")]
        public string Content { get; set; }

        // Bỏ "= DateTime.Now". 
        // Thêm nhãn Identity để C# tự động nhường việc điền thời gian lại cho SQL Server
        [Column("CREATED_AT")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        // Lưu ý: Tên cột này phải khớp chính xác với script SQL của Nhi (ISREAD hoặc IS_READ)
        [Column("ISREAD")]
        public bool IsRead { get; set; } = false;

    }
}