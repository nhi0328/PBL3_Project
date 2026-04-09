using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("VEHICLE_COLORS")]
public class VehicleColor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("COLOR_ID")]
    public int ColorId { get; set; }

    [Required]
    [Column("COLOR_NAME")]
    public string ColorName { get; set; } = string.Empty;

    public virtual ICollection<VehicleType> VehicleTypes { get; set; }

    public VehicleColor()
    {
        VehicleTypes = new HashSet<VehicleType>();
    }
}