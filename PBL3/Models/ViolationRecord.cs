using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("VIOLATION_RECORDS")]
public class ViolationRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("VIOLATION_RECORD_ID")]
    public int ViolationRecordId { get; set; }

    [Column("LICENSE_PLATE")]
    public string? LicensePlate { get; set; }

    [Column("CATEGORY_ID")]
    public int? CategoryId { get; set; } 

    [Column("LAW_ID")]
    public int? LawId { get; set; }

    [Column("VIOLATION_DESCRIPTION")]
    public string? ViolationDescription { get; set; }

    [Column("VIOLATION_DATE")]
    public DateTime? ViolationDate { get; set; }

    [Column("VIOLATION_TIME")]
    public TimeSpan? ViolationTime { get; set; }

    [Column("DEMERIT_POINTS")]
    public string? DemeritPoints { get; set; }

    [Column("WARD_ID")]
    public int? WardId { get; set; }

    [Column("ADDRESS")]
    public string? Address { get; set; } 

    [Column("IMAGE_PATH")]
    public string? ImagePath { get; set; }

    [Column("STATUS")]
    public int Status { get; set; } = 0;
    [NotMapped]
    public string StatusText
    {
        get { return Status == 1 ? "Đã xử lý / Nộp phạt" : "Chưa xử lý"; }
    }

    [Column("LAST_UPDATE")]
    public DateTime? LastUpdate { get; set; }

    // 1. Liên kết tới Phương tiện (Để từ đó truy ra Chủ xe)
    [ForeignKey("LicensePlate")]
    public virtual Vehicle? Vehicle { get; set; }

    // 2. Liên kết tới Danh mục (Loại phương tiện vi phạm)
    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    // 3. Liên kết tới Điều luật (Bảng TRAFFIC_LAWS - Để lấy tên lỗi, mức phạt)
    [ForeignKey("LawId")]
    public virtual TrafficLaw? Law { get; set; } // Lưu ý: Cần tạo model TrafficLaw nếu chưa có

    // 4. Liên kết tới Phường/Xã (Từ Phường sẽ suy ra được Tỉnh/Thành phố)
    [ForeignKey("WardId")]
    public virtual Ward? Ward { get; set; }

    [NotMapped]
    public string? OwnerName => Vehicle?.Owner?.FullName; // Cần thay đổi 'Name' thành tên biến thực tế trong model Customer của bạn

    [NotMapped]
    public string? VehicleBrand => Vehicle?.VehicleType?.VehicleTypeName;

    [NotMapped]
    public string? ViolationDetail => Law?.LawName;
}
