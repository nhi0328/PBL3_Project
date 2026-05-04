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

        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<SystemLog> SystemLogs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 1. ĐÃ SỬA CHUỖI KẾT NỐI CHUẨN XÁC TRỎ VỀ PBL3
                optionsBuilder.UseSqlServer(@"Data Source=NHI\SQLEXPRESS;Initial Catalog=PBL3;Integrated Security=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 2. ÉP TÊN BẢNG CHUẨN XÁC 100% VỚI CƠ SỞ DỮ LIỆU
            modelBuilder.Entity<Admin>().ToTable("ADMINS");
            modelBuilder.Entity<Category>().ToTable("CATEGORIES");
            modelBuilder.Entity<Complaint>().ToTable("COMPLAINTS");
            modelBuilder.Entity<Customer>().ToTable("CUSTOMERS");
            modelBuilder.Entity<DrivingLicense>().ToTable("DRIVING_LICENSES");
            modelBuilder.Entity<Officer>().ToTable("OFFICERS");
            modelBuilder.Entity<Province>().ToTable("PROVINCES");
            modelBuilder.Entity<TrafficLaw>().ToTable("TRAFFIC_LAWS");
            modelBuilder.Entity<TrafficLawDetail>().ToTable("TRAFFIC_LAW_DETAILS");
            modelBuilder.Entity<Vehicle>().ToTable("VEHICLES");
            modelBuilder.Entity<ViolationRecord>().ToTable("VIOLATION_RECORDS");
            modelBuilder.Entity<VehicleColor>().ToTable("VEHICLE_COLORS");
            modelBuilder.Entity<VehicleType>().ToTable("VEHICLE_TYPES");
            modelBuilder.Entity<Ward>().ToTable("WARDS");
            modelBuilder.Entity<Notification>().ToTable("NOTIFICATIONS");
            modelBuilder.Entity<SystemLog>().ToTable("SYSTEM_LOGS");

            // --- CẤU HÌNH KHÓA CHÍNH (GIỮ NGUYÊN CODE CŨ CỦA NHI) ---
            modelBuilder.Entity<Customer>().HasKey(c => c.Cccd);
            modelBuilder.Entity<Officer>().HasKey(o => o.OfficerId);
            modelBuilder.Entity<Vehicle>().HasKey(v => v.LicensePlate);
            modelBuilder.Entity<VehicleType>().HasKey(vt => vt.VehicleTypeId);
            modelBuilder.Entity<DrivingLicense>().HasKey(dl => dl.LicenseNumber);
            modelBuilder.Entity<ViolationRecord>().HasKey(vr => vr.ViolationRecordId);
            modelBuilder.Entity<Complaint>().HasKey(c => c.ComplaintId);
            modelBuilder.Entity<Province>().HasKey(p => p.ProvinceId);
            modelBuilder.Entity<Ward>().HasKey(w => w.WardId);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<VehicleColor>().HasKey(vc => vc.ColorId);

            // Bật Identity
            modelBuilder.Entity<TrafficLaw>().HasKey(t => t.LawId);
            modelBuilder.Entity<TrafficLaw>().Property(t => t.LawId).ValueGeneratedOnAdd();

            modelBuilder.Entity<TrafficLawDetail>().HasKey(td => td.LawDetailId);
            modelBuilder.Entity<TrafficLawDetail>().Property(td => td.LawDetailId).ValueGeneratedOnAdd();

            // Convert status
            modelBuilder.Entity<ViolationRecord>().Property(v => v.Status).HasConversion<string>();
            modelBuilder.Entity<Complaint>().Property(c => c.Status).HasConversion<string>();
            modelBuilder.Entity<DrivingLicense>().Property(d => d.Status).HasConversion<string>();

            // Chặn xóa dây chuyền
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
