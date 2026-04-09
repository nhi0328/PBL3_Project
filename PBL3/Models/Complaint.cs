using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("COMPLAINTS")]
public class Complaint
{
    [Key]
    [Column("COMPLAINT_ID")]
    public int ComplaintId { get; set; }

    [Required]
    [Column("SENDER_CITIZEN_ID")] // Đổi từ CCCD sang đúng tên cột mới
    public string SenderCitizenId { get; set; } = string.Empty;

    [Column("TITLE")]
    public string Title { get; set; } = string.Empty;

    [Column("CONTENT")]
    public string Content { get; set; } = string.Empty;

    [Column("WARD_ID")]
    public int? WardId { get; set; }

    [Column("ADDRESS")]
    public string? Address { get; set; }

    [Column("STATUS")]
    public int Status { get; set; } = 0; // 0: Đang xử lý, 1: Đã xử lý
    [NotMapped]
    public string StatusText
    {
        get { return Status == 1 ? "Đã xử lý" : "Chưa xử lý"; }
    }

    [Column("SUBMIT_DATE")]
    public DateTime SubmitDate { get; set; } = DateTime.Now;

    [Column("LICENSE_PLATE")]
    public string? LicensePlate { get; set; }

    [Column("HANDLING_OFFICER_ID")] // Mã cán bộ tiếp nhận/xử lý
    public string? HandlingOfficerId { get; set; }

    [Column("CATEGORY_ID")] 
    public int? CategoryId { get; set; }

    [Column("IMAGE_PATH")]
    public string? ImagePath { get; set; }

    [Column("OFFICER_RESPONSE")]
    public string? OfficerResponse { get; set; }

    // --- CÁC MỐI QUAN HỆ LIÊN KẾT (FOREIGN KEYS) ---
    [ForeignKey("SenderCitizenId")]
    public virtual Customer? Sender { get; set; }

    [ForeignKey("HandlingOfficerId")]
    public virtual Officer? HandlingOfficer { get; set; }

    [ForeignKey("WardId")]
    public virtual Ward? Ward { get; set; }

    [ForeignKey("LicensePlate")]
    public virtual Vehicle? Vehicle { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    public Complaint() { }

    public Complaint(int id, string senderId, string title, string content, string? licensePlate = null)
    {
        this.ComplaintId = id;
        this.SenderCitizenId = senderId;
        this.Title = title;
        this.Content = content;
        this.LicensePlate = licensePlate;

        this.Status = 0; // Mặc định là 0 (Đang xử lý)
        this.SubmitDate = DateTime.Now; // Tự động lấy ngày giờ hiện tại
        this.OfficerResponse = string.Empty;
    }

    public string Display()
    {
        string statusText = (Status == 1) ? "Đã xử lý" : "Đang xử lý";
        return $"{ComplaintId} | {Title} | {statusText} | Ngày: {SubmitDate:dd/MM/yyyy}";
    }
}