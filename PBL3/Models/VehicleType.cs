using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("VEHICLE_TYPES")]
public class VehicleType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("VEHICLE_TYPE_ID")]
    public int VehicleTypeId { get; set; }

    [Required]
    [Column("VEHICLE_TYPE_NAME")]
    public string VehicleTypeName { get; set; } = string.Empty;

    [Column("CATEGORY_ID")]
    public int? CategoryId { get; set; }

    [Column("MANUFATURE_YEAR")]
    public int? ManufactureYear { get; set; }

    [Column("COLOR_ID")]
    public int? ColorId { get; set; }

    [Column("IMAGE_PATH")]
    public string? ImagePath { get; set; }

    // ==========================================================
    // CÁC MỐI QUAN HỆ LIÊN KẾT (NAVIGATION PROPERTIES)
    // ==========================================================
    public virtual ICollection<Vehicle> Vehicles { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    [ForeignKey("ColorId")]
    public virtual VehicleColor? Color { get; set; }

    // ==========================================================

    public VehicleType()
    {
        // Luôn khởi tạo danh sách rỗng để tránh lỗi văng app (NullReferenceException)
        Vehicles = new HashSet<Vehicle>();
    }
}