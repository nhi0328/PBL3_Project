using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("CATEGORIES")]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CATEGORY_ID")]
    public int CategoryId { get; set; }

    [Required]
    [Column("CATEGORY_NAME")]
    public string CategoryName { get; set; } = string.Empty;

    // 1 Danh mục có thể có nhiều Loại xe
    public virtual ICollection<VehicleType> VehicleTypes { get; set; }

    // 1 Danh mục có thể có nhiều Đơn khiếu nại
    public virtual ICollection<Complaint> Complaints { get; set; }

    public Category()
    {
        // Khởi tạo list trống
        VehicleTypes = new HashSet<VehicleType>();
        Complaints = new HashSet<Complaint>();
    }
}
