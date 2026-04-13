using System;
using System.Linq;
using PBL3.Models;

namespace PBL3.Utils
{
    public static class TrackingHelper
    {
        // 1. Dịch mã hành động (giữ nguyên logic cũ)
        public static string GetActionName(int actionCode)
        {
            switch (actionCode)
            {
                case 1: return "Tạo mới";
                case 2: return "Cập nhật";
                case 3: return "Xóa";
                case 4: return "Xác nhận xử lý";
                default: return "Hành động lạ";
            }
        }

        // 2. Dịch thông tin đối tượng (Dùng tham số string vì TargetValue đã là chữ)
        public static string GetDetailedTargetInfo(string prefix, string value, TrafficSafetyDBContext db)
        {
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(value)) return "Không có dữ liệu";

            switch (prefix.ToUpper())
            {
                // ===== CÁC ĐỐI TƯỢNG CÓ ID LÀ SỐ (Cần ép kiểu về INT) =====
                case "L":
                    if (int.TryParse(value, out int lawId))
                    {
                        var luat = db.TrafficLaws.FirstOrDefault(x => x.LawId == lawId);
                        return luat != null ? $"Luật: {luat.LawName}" : $"Mã luật {lawId} (Đã xóa)";
                    }
                    return "Lỗi mã luật";

                case "P":
                    if (int.TryParse(value, out int complaintId))
                    {
                        var pa = db.Complaints.FirstOrDefault(x => x.ComplaintId == complaintId);
                        return pa != null ? $"Phản ánh: {pa.Title}" : $"Phản ánh #{complaintId} (Đã xóa)";
                    }
                    return "Lỗi mã phản ánh";

                case "B":
                    if (int.TryParse(value, out int recordId))
                    {
                        var bb = db.ViolationRecords.FirstOrDefault(x => x.ViolationRecordId == recordId);
                        if (bb != null)
                        {
                            var tenLuat = db.TrafficLaws.Where(l => l.LawId == bb.LawId).Select(l => l.LawName).FirstOrDefault();
                            return $"Biên bản lỗi: {tenLuat ?? "Không rõ"}";
                        }
                        return $"Biên bản #{recordId} (Đã xóa)";
                    }
                    return "Lỗi mã biên bản";

                // ===== CÁC ĐỐI TƯỢNG CÓ ID LÀ CHUỖI (Dùng thẳng luôn) =====
                case "C":
                    var khach = db.Customers.FirstOrDefault(x => x.Cccd == value);
                    return khach != null ? $"Dân: {khach.FullName}" : $"Người dân {value} (Đã xóa)";

                case "G":
                    var bang = db.DrivingLicenses.FirstOrDefault(x => x.LicenseNumber == value);
                    return bang != null ? $"GPLX: {bang.LicenseNumber}" : $"GPLX {value} (Đã xóa)";

                case "O":
                    var officer = db.Officers.FirstOrDefault(x => x.OfficerId == value);
                    return officer != null ? $"Cán bộ: {officer.OfficerId}" : $"Cán bộ {value} (Đã xóa)";

                case "V":
                    // 1. Tìm chiếc xe dựa vào Biển số
                    var vehicle = db.Vehicles.FirstOrDefault(x => x.LicensePlate == value);

                    if (vehicle != null)
                    {
                        // 2. Tìm tên Loại xe từ bảng VehicleTypes dựa vào VehicleTypeId
                        var loaiXe = db.VehicleTypes.FirstOrDefault(t => t.VehicleTypeId == vehicle.VehicleTypeId);

                        string tenLoai = loaiXe != null ? loaiXe.VehicleTypeName : "Không rõ loại xe";

                        return $"Phương tiện: {vehicle.LicensePlate} ({tenLoai})";
                    }

                    return $"Phương tiện {value} (Đã xóa)";

                default:
                    return $"Đối tượng {prefix}{value}";
            }
        }

        // Dịch mã Role sang chữ
        public static string GetRoleName(int roleCode)
        {
            switch (roleCode)
            {
                case 1: return "Quản trị viên"; // Admin
                case 2: return "Cán bộ";        // Officer
                case 3: return "Người dân";     // Customer
                default: return "Không xác định";
            }
        }
    }
}