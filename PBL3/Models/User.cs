using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    [Table("USERS")] // Khớp với tên bảng USERS trong SQL
    public partial class User
    {
        // 1. CONSTRUCTORS
        public User()
        {
            Vehicles = new HashSet<Vehicle>();
        }

        public User(string cccd, string fullName, string phone, string email, DateTime dob, string gender, string pass)
        {
            this.Cccd = cccd;
            this.FullName = fullName;
            this.Phone = phone;
            this.Email = email;
            this.Dob = dob;
            this.Gender = gender;
            this.PassHash = pass;
            this.Vehicles = new HashSet<Vehicle>();
        }

        // 2. PROPERTIES (Đã map đúng tên cột SQL)
        [Key]
        [Column("CCCD")]
        public virtual string Cccd { get; set; } = string.Empty;

        [Column("FULL_NAME")]
        public string FullName { get; set; } = string.Empty;

        [Column("PASS_HASH")]
        public string PassHash { get; set; } = string.Empty;

        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [Column("PHONE")]
        public string Phone { get; set; } = string.Empty;

        [Column("DOB")]
        public DateTime? Dob { get; set; }

        [Column("GENDER")]
        public string Gender { get; set; } = string.Empty;

        [Column("IMAGE_PATH")]
        public string ImagePath { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; }

        // 3. VIRTUAL METHODS
        public virtual string GetRole() => "USER";
        public virtual string Display() => $"{FullName} - CCCD: {Cccd}";
    }
}