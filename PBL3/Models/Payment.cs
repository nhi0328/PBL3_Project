using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models; // Đảm bảo đúng Namespace để hết lỗi CS0246

[Table("PAYMENTS_HISTORY")] // Ánh xạ đúng tên bảng trong SQL
public class Payment
{
    [Key]
    [Column("PAYMENT_ID")] // Ánh xạ đúng tên cột PK trong SQL
    public string PaymentId { get; set; } = string.Empty;

    [Column("TICKET_ID")]
    public string TicketId { get; set; } = string.Empty;

    [Column("CCCD")]
    public string CCCD { get; set; } = string.Empty;

    [Column("VEHICLE_ID")]
    public string VehicleId { get; set; } = string.Empty;

    [Column("AMOUNT")]
    public double Amount { get; set; } // SQL dùng float nên C# dùng double là chuẩn nhất

    [Column("METHOD")]
    public string Method { get; set; } = string.Empty;

    [Column("DATE")]
    public DateTime Date { get; set; } = DateTime.Now;

    // Navigation properties - Các mối quan hệ liên kết
    [ForeignKey("TicketId")]
    public virtual Ticket Ticket { get; set; }

    [ForeignKey("CCCD")]
    public virtual User User { get; set; }

    public Payment() { }

    public Payment(string id, Ticket ticket, double amount, string method, string cccd, string vehicleId)
    {
        PaymentId = id;
        Ticket = ticket;
        TicketId = ticket.TicketId;
        Amount = amount;
        Method = method;
        CCCD = cccd;
        VehicleId = vehicleId;
        Date = DateTime.Now;

        // Tự động thêm bản ghi này vào danh sách thanh toán của biên bản đó
        ticket.Payments ??= new List<Payment>();
        ticket.Payments.Add(this);
    }

    public string Display()
    {
        return $"{PaymentId} | {Amount} | {Method} | {Date:dd/MM/yyyy}";
    }
}