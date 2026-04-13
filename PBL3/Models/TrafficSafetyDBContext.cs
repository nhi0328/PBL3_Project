using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PBL3.Models
{
    public partial class TrafficSafetyDBContext : DbContext
    {
        public TrafficSafetyDBContext()
        {
        }

        public TrafficSafetyDBContext(DbContextOptions<TrafficSafetyDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Complaint> Complaints { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<DrivingLicense> DrivingLicenses { get; set; } = null!;
        public virtual DbSet<Officer> Officers { get; set; } = null!;
        public virtual DbSet<Province> Provinces { get; set; } = null!;
        public virtual DbSet<TrafficLaw> TrafficLaws { get; set; } = null!;
        public virtual DbSet<TrafficLawDetail> TrafficLawDetails { get; set; } = null!;
        public virtual DbSet<Vehicle> Vehicles { get; set; } = null!;
        public virtual DbSet<ViolationRecord> ViolationRecords { get; set; } = null!;
        public virtual DbSet<VehicleColor> VehicleColors { get; set; } = null!;
        public virtual DbSet<VehicleType> VehicleTypes { get; set; } = null!;
        public virtual DbSet<Ward> Wards { get; set; } = null!;
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TrafficSafetyDB;Integrated Security=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- CẤU HÌNH KHÓA CHÍNH (PRIMARY KEYS) CŨ ---
            modelBuilder.Entity<Customer>().HasKey(c => c.Cccd);
            modelBuilder.Entity<Officer>().HasKey(o => o.OfficerId);
            modelBuilder.Entity<Vehicle>().HasKey(v => v.LicensePlate);
            modelBuilder.Entity<VehicleType>().HasKey(vt => vt.VehicleTypeId);
            modelBuilder.Entity<DrivingLicense>().HasKey(dl => dl.LicenseNumber);
            modelBuilder.Entity<ViolationRecord>().HasKey(vr => vr.ViolationRecordId);
            modelBuilder.Entity<Complaint>().HasKey(c => c.ComplaintId);

            // --- CẤU HÌNH KHÓA CHÍNH & TỰ ĐỘNG TĂNG CHO CÁC BẢNG MỚI ---

            modelBuilder.Entity<Province>().HasKey(p => p.ProvinceId);
            modelBuilder.Entity<Ward>().HasKey(w => w.WardId);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<VehicleColor>().HasKey(vc => vc.ColorId);

            // Đảm bảo bật tính năng tự động tăng (Identity) cho LAW_ID
            modelBuilder.Entity<TrafficLaw>()
                .HasKey(t => t.LawId);
            modelBuilder.Entity<TrafficLaw>()
                .Property(t => t.LawId)
                .ValueGeneratedOnAdd();

            // Đảm bảo bật tính năng tự động tăng (Identity) cho DETAIL_ID
            modelBuilder.Entity<TrafficLawDetail>()
                .HasKey(td => td.LawDetailId);
            modelBuilder.Entity<TrafficLawDetail>()
                .Property(td => td.LawDetailId)
                .ValueGeneratedOnAdd();
                
            // Convert int <-> string cho các cột STATUS là nvarchar trong CSDL
            modelBuilder.Entity<ViolationRecord>()
                .Property(v => v.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<DrivingLicense>()
                .Property(d => d.Status)
                .HasConversion<string>();

            // --- CẤU HÌNH LIÊN KẾT (FLUENT API) ĐỂ TRÁNH LỖI XÓA DÂY CHUYỀN ---
            // (Khi thiết kế DB phức tạp, có nhiều khóa ngoại trỏ về cùng 1 bảng, 
            // SQL Server rất hay báo lỗi "multiple cascade paths". 
            // Lệnh dưới đây giúp tắt xóa dây chuyền để Database an toàn)

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
