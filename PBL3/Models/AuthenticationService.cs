using System;
using System.Linq;
using PBL3.Models;

namespace PBL3.Models;

public static class AuthenticationService
{
    public static User Login(string identifier, string password)
    {
        // 1. SỬA TÊN CONTEXT: Phải là TrafficSafetyDBContext mới có BadgeNumber
        using TrafficSafetyDBContext db = new TrafficSafetyDBContext();

        string type = DetectInputType(identifier);

        // 2. KIỂM TRA OFFICER
        // Trong file AuthenticationService.cs, đoạn kiểm tra Officer:
        var officer = db.Officers.FirstOrDefault(o => o.Cccd == identifier && o.PassHash == password);

        if (officer != null)
        {
            // Kiểm tra xem có phải 4 Admin mặc định không
            string[] adminIds = { "AD01", "AD02", "AD03", "AD04" }; // Thay mã thật của bạn vào đây

            if (adminIds.Contains(officer.BadgeNumber))
                return new Admin(officer.BadgeNumber, officer.FullName, officer.PassHash);

            return new Officer(officer.BadgeNumber, officer.FullName, officer.PassHash);
        }

        // 3. KIỂM TRA USER (Công dân)
        // Đổi u.Id thành u.Cccd và u.Password thành u.PassHash
        return db.Users.FirstOrDefault(u =>
            ((type == "Email" && u.Email == identifier) ||
             (type == "Phone" && u.Phone == identifier) ||
             (type == "Id" && u.Cccd == identifier)) &&
            u.PassHash == password);
    }

    private static string DetectInputType(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Id";
        if (input.Contains('@')) return "Email";
        if (input.All(char.IsDigit) && input.Length <= 11) return "Phone";
        return "Id";
    }
}