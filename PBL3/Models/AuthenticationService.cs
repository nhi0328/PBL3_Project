using System;
using System.Linq;

namespace PBL3.Models;

public static class AuthenticationService
{
    /// <summary>
    /// Hàm đăng nhập kiểm tra qua 3 bảng độc lập: ADMINS, OFFICERS, CUSTOMERS
    /// </summary>
    /// <returns>Trả về object thuộc kiểu Admin, Officer hoặc Customer nếu thành công</returns>
    public static object? Login(string identifier, string password)
    {
        // Sử dụng Context mới đã chốt
        using TrafficSafetyDBContext db = new TrafficSafetyDBContext();

        string type = DetectInputType(identifier);

        // 1. KIỂM TRA QUẢN TRỊ VIÊN (Bảng ADMINS)
        // Admin đăng nhập bằng AdminId (ví dụ: AD01)
        var admin = db.Admins.FirstOrDefault(a => a.AdminId == identifier && a.Password == password);
        if (admin != null)
        {
            if (admin.Status == 1) return admin; // Chỉ cho phép đăng nhập nếu tài khoản đang Active
            return null;
        }

        // 2. KIỂM TRA CÁN BỘ (Bảng OFFICERS)
        // Cán bộ đăng nhập bằng Mã số hiệu (OfficerId)
        var officer = db.Officers.FirstOrDefault(o => o.OfficerId == identifier && o.Password == password);
        if (officer != null) return officer;

        // 3. KIỂM TRA CÔNG DÂN (Bảng CUSTOMERS)
        // Công dân có thể đăng nhập linh hoạt bằng Email, Số điện thoại hoặc CCCD
        var customer = db.Customers.FirstOrDefault(u =>
            ((type == "Email" && u.Email == identifier) ||
             (type == "Phone" && u.Phone == identifier) ||
             (type == "Id" && u.Cccd == identifier)) &&
            u.Password == password);

        if (customer != null) return customer;

        // Nếu không khớp với ai trong cả 3 bảng
        return null;
    }

    /// <summary>
    /// Tự động phân loại dữ liệu người dùng nhập vào (Email, Phone hay ID)
    /// </summary>
    private static string DetectInputType(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "Unknown";

        if (input.Contains("@")) return "Email";

        // Nếu chỉ toàn số và có độ dài của SĐT (10-11 số)
        if (input.All(char.IsDigit) && (input.Length == 10 || input.Length == 11))
            return "Phone";

        return "Id"; // Mặc định coi là CCCD hoặc Mã số hiệu (AdminId/OfficerId)
    }
}