using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; 

[Table("DRIVING_LICENSES")]
public class DrivingLicense
{
    [Key]
    [Column("LICENSE_NUMBER")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Column("CCCD")]
    public string Cccd { get; set; } = string.Empty;

    [Column("LICENSE_ID")]
    public string? LicenseId { get; set; } // Hạng GPLX (Ví dụ: A1, B2...)

    [Column("ISSUE_DATE")]
    public DateTime IssueDate { get; set; }

    [Column("EXPIRY_DATE")]
    public DateTime? ExpiryDate { get; set; }
    [NotMapped]
    public string ExpiryDateText => ExpiryDate.HasValue ? ExpiryDate.Value.ToString("dd/MM/yyyy") : "Không thời hạn";

    [Column("POINTS")]
    public int Points { get; set; } // Điểm bằng lái hiện tại (dùng cho Trigger trừ điểm)

    [Column("STATUS")]
    public int Status { get; set; } = 1;
    [NotMapped]
    public string StatusText
    {
        get
        {
            return Status switch
            {
                1 => "Đang hoạt động",
                2 => "Tước bằng",
                3 => "Hết hạn",
                _ => "Không xác định"
            };
        }
    }

    [Column("DEMERIT_POINTS")]
    public int DemeritPoints { get; set; } = 0; // Số điểm đã bị trừ do vi phạm

    // Navigation Properties - Mối quan hệ liên kết
    [ForeignKey("Cccd")]
    public virtual Customer? CustomerInfo { get; set; }


    public DrivingLicense() { }

    public DrivingLicense(string licenseNumber, string cccd, string licenseId, DateTime issueDate, DateTime expiryDate)
    {
        this.LicenseNumber = licenseNumber;
        this.Cccd = cccd;
        this.LicenseId = licenseId;
        this.IssueDate = issueDate;
        this.ExpiryDate = CalculateExpiryDate(licenseId, issueDate);

        this.Points = 12;
        this.Status = 1; // Mặc định khi mới tạo là 1: Đang hoạt động
        this.DemeritPoints = 0;
    }

    private DateTime? CalculateExpiryDate(string? licenseId, DateTime issue)
    {
        if (string.IsNullOrEmpty(licenseId)) return null;

        string type = licenseId.Trim().ToUpper();

        // Phân loại các hạng bằng theo nhóm
        string[] noExpiry = { "A1", "A", "B1" };
        string[] tenYears = { "B", "C1" };
        string[] fiveYears = { "C", "D1", "D2", "D", "BE", "C1E", "CE", "D1E", "D2E", "DE" };

        if (noExpiry.Contains(type)) return null; // Vô thời hạn
        if (tenYears.Contains(type)) return issue.AddYears(10); // Cộng 10 năm
        if (fiveYears.Contains(type)) return issue.AddYears(5); // Cộng 5 năm

        return null;
    }

    public string Display()
    {
        return $"GPLX: {LicenseNumber} | Hạng: {LicenseId} | CCCD: {Cccd} | Trạng thái: {StatusText}";
    }
}