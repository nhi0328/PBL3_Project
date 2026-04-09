using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("WARDS")]
public class Ward
{
    [Key]
    [Column("WARD_ID")]
    public int WardId { get; set; }

    [Column("WARD_NAME")]
    public string WardName { get; set; } = string.Empty;

    // Khóa ngoại liên kết tới bảng PROVINCES
    [Column("PROVINCE_ID")]
    public int ProvinceId { get; set; }

    // --- CÁC MỐI QUAN HỆ LIÊN KẾT ---

    // Liên kết để biết Phường này thuộc Tỉnh nào
    [ForeignKey("ProvinceId")]
    public virtual Province? Province { get; set; }

    // 1 Phường có thể có nhiều Đơn khiếu nại
    public virtual ICollection<Complaint> Complaints { get; set; }

    public Ward()
    {
        Complaints = new HashSet<Complaint>();
    }
}