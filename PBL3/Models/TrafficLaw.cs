using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("TRAFFIC_LAWS")]
public class TrafficLaw
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("LAW_ID")]
    public int LawId { get; set; }

    [Required]
    [Column("LAW_NAME")]
    public string LawName { get; set; } = string.Empty;

    // 1 Điều luật có thể có nhiều Chi tiết mức phạt (cho nhiều loại xe khác nhau)
    public virtual ICollection<TrafficLawDetail> Details { get; set; }

    // 1 Điều luật có thể xuất hiện trong nhiều Hồ sơ vi phạm
    public virtual ICollection<ViolationRecord> ViolationRecords { get; set; }

    public TrafficLaw()
    {
        // Khởi tạo list trống
        Details = new HashSet<TrafficLawDetail>();
        ViolationRecords = new HashSet<ViolationRecord>();
    }
}