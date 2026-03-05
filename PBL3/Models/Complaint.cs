using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; 

[Table("COMPLAINTS")] 
public class Complaint
{
    [Key]
    [Column("COMPLAINT_ID")] 
    public string ComplaintId { get; set; } = string.Empty;

    [Column("TICKET_ID")]
    public string TicketId { get; set; } = string.Empty;

    [Column("CCCD")]
    public string CCCD { get; set; } = string.Empty;

    [Column("CONTENT")]
    public string Content { get; set; } = string.Empty;

    [Column("STATUS")]
    public string Status { get; set; } = string.Empty;

    // Navigation properties - Các mối quan hệ liên kết
    [ForeignKey("TicketId")]
    public virtual Ticket Ticket { get; set; } // Đổi từ Violation sang Ticket

    [ForeignKey("CCCD")]
    public virtual User User { get; set; }

    // Lưu ý: Đã xóa List<ComplaintHistory> vì bảng này không có trong SQL mới

    public Complaint() { }

    public Complaint(string id, Ticket ticket, User user, string content)
    {
        this.ComplaintId = id;
        this.Ticket = ticket;
        this.TicketId = ticket.TicketId;
        this.User = user;
        this.CCCD = user.Cccd;
        this.Content = content;
        this.Status = "ĐANG XỬ LÝ";

        // Tự động thêm vào danh sách khiếu nại của biên bản đó
        ticket.Complaints ??= new List<Complaint>();
        ticket.Complaints.Add(this);
    }

    public string Display()
    {
        return $"{ComplaintId} | {Status} | {Content}";
    }
}