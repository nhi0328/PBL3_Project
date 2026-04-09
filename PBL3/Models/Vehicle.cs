using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("VEHICLES")]
public class Vehicle
{
    [Key]
    [Column("LICENSE_PLATE")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [Column("CCCD")]
    public string Cccd { get; set; } = string.Empty;

    [Column("VEHICLE_TYPE_ID")]
    public int? VehicleTypeId { get; set; }

    [Column("SHASSIS_NUMBER")]
    public string? ShassisNumber { get; set; }

    [Column("ENGINE_NUMBER")]
    public string? EngineNumber { get; set; }

    [Column("REGISTRATION_DATE")]
    public DateTime? RegistrationDate { get; set; }

    [Column("STATUS")]
    public int Status { get; set; } = 1; // 1: Đang sử dụng, 2: Tạm giữ...

    // --- LIÊN KẾT KHÓA NGOẠI (FOREIGN KEY) ---

    // 1. Liên kết tới Chủ xe (Người dân) thông qua CCCD
    [ForeignKey("Cccd")]
    public virtual Customer? Owner { get; set; }

    // 2. Liên kết tới Loại xe thông qua VEHICLE_TYPE_ID
    [ForeignKey("VehicleTypeId")]
    public virtual VehicleType? VehicleType { get; set; }

    // 3. Mối quan hệ 1-Nhiều: 1 xe có thể có nhiều Lỗi vi phạm
    public virtual ICollection<ViolationRecord> ViolationRecords { get; set; }

    // 4. Mối quan hệ 1-Nhiều: 1 xe có thể bị Phản ánh nhiều lần
    public virtual ICollection<Complaint> Complaints { get; set; }

    // ==========================================================

    public Vehicle()
    {
        // Luôn khởi tạo danh sách rỗng để tránh lỗi NullReferenceException khi Add
        ViolationRecords = new HashSet<ViolationRecord>();
        Complaints = new HashSet<Complaint>();
    }

    public Vehicle(string licensePlate, string cccd, int? vehicleTypeId, string? shassisNumber, string? engineNumber, DateTime? registrationDate, int status = 1)
    {
        this.LicensePlate = licensePlate;
        this.Cccd = cccd;
        this.VehicleTypeId = vehicleTypeId;
        this.ShassisNumber = shassisNumber;
        this.EngineNumber = engineNumber;
        this.RegistrationDate = registrationDate;
        this.Status = status;

        this.ViolationRecords = new HashSet<ViolationRecord>();
        this.Complaints = new HashSet<Complaint>();
    }

    public string Display()
    {
        // Xử lý ngày đăng ký nếu bị null
        string dateStr = RegistrationDate.HasValue ? RegistrationDate.Value.ToString("dd/MM/yyyy") : "Chưa cập nhật";

        // Nếu đã Include bảng VehicleType thì lấy tên, chưa thì lấy ID
        string typeName = VehicleType != null ? VehicleType.VehicleTypeName : VehicleTypeId?.ToString() ?? "N/A";

        string statusStr = Status == 1 ? "Đang sử dụng" : (Status == 2 ? "Tạm giữ" : "Thu xe vĩnh viễn");

        return $"Biển số: {LicensePlate} | Loại xe: {typeName} | CCCD: {Cccd} | Đăng ký: {dateStr}";
    }
}