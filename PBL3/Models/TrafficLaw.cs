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

    [Column("VIOLATION_DESCRIPTION")]
    public string ViolationDescription { get; set; } = string.Empty;

    [Column("FINE_RANGE")]
    public string FineRange { get; set; } = string.Empty;

    [Column("LAW_REFERENCE")]
    public string LawReference { get; set; } = string.Empty;

    [Column("VEHICLE_TYPE")]
    public string VehicleType { get; set; } = string.Empty; // "Xe máy" hoặc "Ô tô"

    public TrafficLaw() { }

    public TrafficLaw(string desc, string fine, string refe, string type)
    {
        this.ViolationDescription = desc;
        this.FineRange = fine;
        this.LawReference = refe;
        this.VehicleType = type;
    }
}