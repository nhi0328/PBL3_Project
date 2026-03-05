namespace PBL3.Models;

public class Admin : User
{
    public Admin(string id, string name, string pass)
    {
        this.Cccd = id;
        this.FullName = name;
        this.PassHash = pass;
    }

    public override string GetRole() => "ADMIN";
    public override string Display() => $"[Quản trị hệ thống] {FullName}";
}