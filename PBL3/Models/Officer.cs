using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("OFFICERS")]
public class Officer
{

    [Key]
    [Column("OFFICER_ID")]
    public string OfficerId { get; set; } = string.Empty;

    [Required]
    [Column("CCCD")]
    public string Cccd { get; set; } = string.Empty;

    [Required]
    [Column("PASSWORD")]
    public string Password { get; set; } = string.Empty;

    [ForeignKey("Cccd")]
    public virtual Customer? CustomerInfo { get; set; }

    public Officer() { }

    public Officer(string officerId, string cccd, string password)
    {
        OfficerId = officerId;
        Cccd = cccd;
        Password = password;
    }

    public string GetRole()
    {
        return "OFFICER";
    }

    public string Display()
    {
        string name = CustomerInfo != null ? CustomerInfo.FullName : "Chưa tải tên";
        return $"[Cán bộ] {name} - SH: {OfficerId}";
    }
}