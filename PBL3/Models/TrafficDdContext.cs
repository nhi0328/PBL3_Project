using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace PBL3.Models; // Đảm bảo đúng Namespace để nhận diện các Model

public class TrafficDbContext : DbContext
{
    // 1. Đăng ký tất cả 11 bảng dữ liệu theo file SQL mới
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Officer> Officers { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<TrafficLaw> TrafficLaws { get; set; } = null!;
    public DbSet<ViolationType> ViolationTypes { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<DrivingLicense> DrivingLicenses { get; set; } = null!;
    public DbSet<Complaint> Complaints { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Cập nhật chuỗi kết nối đến Database QuanLyViPham
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=TrafficSafetyDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}