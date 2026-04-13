using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    [Table("SYSTEM_LOGS")]
    public class SystemLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("LOG_ID")]
        public int LogId { get; set; }

        [Required]
        [Column("ROLE")]
        public int Role { get; set; } // Đổi từ string sang int

        [Required]
        [StringLength(50)]
        [Column("ID")]
        public string Id { get; set; } // ID người thực hiện

        [Column("TIME")]
        public DateTime Time { get; set; } = DateTime.Now;

        [Required]
        [Column("ACTION")]
        public int Action { get; set; }

        [Required]
        [MaxLength(1)]
        [Column("TARGET_PREFIX")]
        public string TargetPrefix { get; set; }

        [Required]
        [StringLength(50)] 
        [Column("TARGET_VALUE")]
        public string TargetValue { get; set; } 
    }
}