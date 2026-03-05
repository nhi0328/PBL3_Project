using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; 

[Table("TICKETS")]
public class Ticket
{
    [Key]
    [Column("TICKET_ID")]
    public string TicketId { get; set; } = string.Empty;

    [Column("BADGE_NUMBER")]
    public string BadgeNumber { get; set; } = string.Empty;

    [Column("VEHICLE_ID")]
    public string VehicleId { get; set; } = string.Empty;

    [Column("CCCD")]
    public string CCCD { get; set; } = string.Empty;

    [Column("VIOLATION_TIME")]
    public DateTime ViolationTime { get; set; }

    [Column("LOCATION")]
    public string Location { get; set; } = string.Empty;

    [Column("STATUS")]
    public string Status { get; set; } = string.Empty;

    [Column("TOTAL_AMOUNT")]
    public double TotalAmount { get; set; }

    // Liên kết với các loại lỗi trong biên bản này (N-N)
    public virtual ICollection<ViolationType> ViolationTypes { get; set; } = new List<ViolationType>();
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}