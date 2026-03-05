using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; // Đảm bảo đúng Namespace để xóa lỗi CS0246

[Table("DRIVING_LICENSES")] // Ánh xạ đúng tên bảng trong SQL
public class DrivingLicense
{
    [Key]
    [Column("LICENSE_ID")] // Khớp với PK trong SQL
    public string LicenseId { get; set; } = string.Empty;

    [Column("CCCD")]
    public string CCCD { get; set; } = string.Empty;

    [Column("VEHICLE_ID")]
    public string VehicleId { get; set; } = string.Empty;

    [Column("CLASS")]
    public string LicenseClass { get; set; } = string.Empty;

    [Column("ISSUE_DATE")]
    public DateTime IssueDate { get; set; }

    [Column("EXPIRY_DATE")]
    public DateTime ExpiryDate { get; set; } // Thuộc tính mới từ SQL

    [Column("CUR_POINT")]
    public int CurPoint { get; set; } // Điểm bằng lái hiện tại (dùng cho Trigger trừ điểm)

    [Column("STATUS")]
    public string Status { get; set; } = string.Empty;

    // Navigation Properties - Mối quan hệ liên kết
    [ForeignKey("CCCD")]
    public virtual User User { get; set; }

    [ForeignKey("VehicleId")]
    public virtual Vehicle Vehicle { get; set; }

    public DrivingLicense() { }

    public DrivingLicense(string id, string cls, User user, DateTime expiry)
    {
        this.LicenseId = id;
        this.LicenseClass = cls;
        this.User = user;
        this.CCCD = user.Cccd; // Đổi .Id thành .Cccd
        this.IssueDate = DateTime.Now;
        this.ExpiryDate = expiry;
    }

    public string Display()
    {
        return $"{LicenseId} | {LicenseClass} | Điểm: {CurPoint} | {Status}";
    }
}