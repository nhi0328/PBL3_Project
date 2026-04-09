using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("CUSTOMERS")]
public class Customer
{
    [Key]
    [Column("CCCD")]
    public string Cccd { get; set; } = string.Empty;

    [Required]
    [Column("FULL_NAME")]
    public string FullName { get; set; } = string.Empty;

    [Column("GENDER")]
    public string? Gender { get; set; }

    [Column("DOB")]
    public DateTime? Dob { get; set; }

    [Column("EMAIL")]
    public string? Email { get; set; }

    [Column("PHONE")]
    public string? Phone { get; set; }

    [Required]
    [Column("PASSWORD")]
    public string Password { get; set; } = string.Empty;

    [Column("AVATAR")]
    public string? Avatar { get; set; }

    public Customer() { }

    public Customer(string cccd, string fullName, string gender, DateTime? dob, string email, string phone, string password)
    {
        Cccd = cccd;
        FullName = fullName;
        Gender = gender;
        Dob = dob;
        Email = email;
        Phone = phone;
        Password = password;
    }

    public string GetRole()
    {
        return "CUSTOMER";
    }

    public string Display()
    {
        return $"[Công dân] {FullName} - CCCD: {Cccd}";
    }
}