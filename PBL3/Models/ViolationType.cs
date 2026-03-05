using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("VIOLATION_TYPES")]
public class ViolationType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("VIOLATION_TYPE_ID")] // Cột ID Nhi muốn thêm ở trước
    public int ViolationTypeId { get; set; }

    [Column("VIOLATION_NAME")]
    public string ViolationName { get; set; } = string.Empty; // Ví dụ: "Hệ thống phanh không an toàn"

    [Column("LEGAL_NOTE")]
    public string? LegalNote { get; set; } // Ghi chú pháp lý (Cột 2 trong file của Nhi)

    public ViolationType() { }

    public ViolationType(string name, string note)
    {
        this.ViolationName = name;
        this.LegalNote = note;
    }
}