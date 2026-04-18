#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("ADMINS")]
public class Admin
{
    // --- 5 CỘT CHÍNH XÁC CÓ TRONG DATABASE ---

    [Key]
    [Column("ADMIN_ID")] // Khớp đúng tên AMIN_ID của Nhi
    public string AdminId { get; set; } = string.Empty;

    [Column("USERNAME")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("FULL_NAME")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Column("PASS_HASH")] // Khớp đúng tên PASH_HASH của Nhi
    public string Password { get; set; } = string.Empty;

    [Column("IMAGE_PATH")]
    public string? ImagePath { get; set; }


    // --- CÁC BIẾN CŨ BỊ DƯ (THÊM [NotMapped] ĐỂ KHÔNG BÁO LỖI) ---

    [NotMapped]
    public string? Email { get; set; }

    [NotMapped]
    public string? Phone { get; set; }

    [NotMapped]
    public string Role { get; set; } = "SuperAdmin";

    [NotMapped]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    public DateTime? LastLogin { get; set; }

    [NotMapped]
    public int Status { get; set; } = 1;

    // --- CONSTRUCTOR ---

    public Admin() { }

    public Admin(string id, string name, string email, string phone, string pass)
    {
        this.AdminId = id;
        this.FullName = name;
        this.Email = email;
        this.Phone = phone;
        this.Password = pass;
    }

    public string GetRole()
    {
        return "ADMIN";
    }
}