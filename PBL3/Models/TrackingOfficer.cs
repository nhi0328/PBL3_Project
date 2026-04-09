using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models // Nhớ sửa lại namespace cho đúng với Project của Nhi
{
    [Table("TRACKING_OFFICER")] // Bắt buộc phải trùng tên với bảng trong SQL
    public class TrackingOfficer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("ID_OFFICER")] // Ánh xạ đúng tên cột trong SQL
        public string IdOfficer { get; set; }

        [Column("TIME")]
        public DateTime Time { get; set; } = DateTime.Now;

        [Required]
        [Column("ACTION")]
        public int Action { get; set; } // Sẽ lưu các số 1, 2, 3, 4

        [Required]
        [StringLength(50)]
        [Column("TARGET_ID")]
        public string TargetId { get; set; }
    }
}