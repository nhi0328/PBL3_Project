using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("TRAFFIC_LAW_DETAILS")]
public class TrafficLawDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("LAW_DETAIL_ID")]
    public int LawDetailId { get; set; }

    [Required]
    [Column("LAW_ID")]
    public int LawId { get; set; }

    [Column("CATEGORY_ID")]
    public int? CategoryId { get; set; } // "Xe máy", "Ô tô"...

    [Column("FIND_AMOUNT")]
    public string? FineAmount { get; set; }

    [Column("DEMERIT_POINTS")]
    public int? DemeritPoints { get; set; }

    [Column("DECREE")]
    public string? Decree { get; set; } // Nghị định (vd: "Nghị định 100/2019/NĐ-CP")

    [Column("ISSUE_DATE")]
    public DateTime? IssueDate { get; set; }

    [Column("EFECTIVE_DATE")]
    public DateTime? EffectiveDate { get; set; }

    // 1. Liên kết ngược lại với bảng TRAFFIC_LAWS
    [ForeignKey("LawId")]
    public virtual TrafficLaw? TrafficLaw { get; set; }

    // 2. Liên kết sang bảng CATEGORIES (Để lấy chữ "Ô tô" hay "Xe máy")
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }
}