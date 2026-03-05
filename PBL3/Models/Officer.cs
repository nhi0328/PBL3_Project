using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("OFFICERS")]
public partial class Officer : User
{
    // Không dùng override Cccd ở đây nữa để tránh lỗi LINQ

    [NotMapped] // Nhãn này bảo EF: "Đừng tìm cột BadgeNumber trong SQL, tôi chỉ dùng trong code thôi"
    public string BadgeNumber
    {
        get => Cccd;
        set => Cccd = value;
    }

    public Officer() : base() { }

    public Officer(string badge, string name, string pass)
        : base(badge, name, "", "", DateTime.Now, "", pass)
    {
        this.Cccd = badge;
    }

    public override string GetRole() => "OFFICER";
    public override string Display() => $"[Cán bộ] {FullName} - SH: {Cccd}";
}