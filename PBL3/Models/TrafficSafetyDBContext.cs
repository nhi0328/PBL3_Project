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

        public virtual DbSet<Officer> Officers { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }

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
            // 1. Cấu hình cho lớp Cha (User)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.Cccd); // Đây là Khóa chính duy nhất của cả hệ thống
                entity.Property(e => e.Cccd).HasColumnName("CCCD");
            });

            // 2. Cấu hình cho lớp Con (Officer)
            modelBuilder.Entity<Officer>(entity => {
                entity.ToTable("OFFICERS");
                entity.Property(o => o.Cccd).HasColumnName("BADGE_NUMBER");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("VEHICLES");
                entity.HasKey(e => e.LicensePlate); // Khai báo LicensePlate là Khóa chính
            });

            // Cấu hình bảng Luật
            modelBuilder.Entity<TrafficLaw>(entity => {
                entity.ToTable("TRAFFIC_LAWS");
                entity.HasKey(e => e.LawId);
                entity.Property(e => e.VehicleType).HasColumnName("VEHICLE_TYPE");
            });

            // Cấu hình bảng Loại vi phạm(ViolationType) -THÊM MỚI TẠI ĐÂY
            modelBuilder.Entity<ViolationType>(entity => {
                entity.ToTable("VIOLATION_TYPES");
                entity.HasKey(e => e.ViolationTypeId); // Khóa chính ID tự tăng
            });

            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
