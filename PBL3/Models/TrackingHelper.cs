using System;
using PBL3.Models; // Namespace chứa các bảng SQL của Nhi
using System.Linq;

namespace PBL3.Utils
{
    public static class TrackingHelper
    {
        // 1. DỊCH MÃ HÀNH ĐỘNG (1, 2, 3, 4)
        public static string GetActionName(int actionCode)
        {
            switch (actionCode)
            {
                case 1: return "Tạo mới";
                case 2: return "Cập nhật";
                case 3: return "Xóa";
                case 4: return "Xác nhận xử lý";
                default: return "Không xác định";
            }
        }

        // 2. TÁCH CHỮ CÁI ĐỂ NHẬN DIỆN BẢNG VÀ DỊCH TÊN ĐỐI TƯỢNG
        public static string GetTargetDisplayName(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return "Lỗi dữ liệu";

            char prefix = targetId[0]; // Lấy chữ cái đầu tiên (P, B, L, C, G)

            switch (prefix)
            {
                case 'P': return $"Phản ánh ({targetId})";
                case 'B': return $"Biên bản phạt ({targetId})";
                case 'L': return $"Điều luật ({targetId})";
                case 'C': return $"Người dân ({targetId})";
                case 'G': return $"Bằng lái ({targetId})";
                default: return $"Đối tượng khác ({targetId})";
            }
        }

        public static string GetDetailedTargetInfo(string targetId, TrafficSafetyDBContext db)
        {
            // Chặn lỗi nếu targetId rỗng hoặc bị thiếu số
            if (string.IsNullOrEmpty(targetId) || targetId.Length < 2) return "Lỗi dữ liệu";

            char prefix = targetId[0]; // Lấy chữ cái đầu (P, B, L, C, G)

            // CẮT BỎ CHỮ CÁI ĐẦU, chỉ lấy phần đuôi. Ví dụ: "L123" -> "123", "C079" -> "079"
            string realIdString = targetId.Substring(1);

            switch (prefix)
            {
                case 'L':
                    // 💡 Bảng Luật (LawId là INT): Cần ép kiểu từ String sang Int
                    if (int.TryParse(realIdString, out int lawIdInt))
                    {
                        var luat = db.TrafficLaws.FirstOrDefault(x => x.LawId == lawIdInt);
                        return luat != null ? $"Luật: {luat.LawName}" : "Luật (Đã xóa)";
                    }
                    return "Mã luật bị lỗi định dạng";

                case 'C':
                    // 💡 Bảng Khách hàng (Cccd là STRING): So sánh chuỗi trực tiếp, KHÔNG cần ép sang Int
                    var khach = db.Customers.FirstOrDefault(x => x.Cccd == realIdString);
                    return khach != null ? $"Người dân: {khach.FullName}" : "Người dân (Đã xóa)";

                case 'B':
                    // 💡 Bảng Biên bản: Giả sử ViolationRecordId là kiểu INT 
                    // (Nếu của Nhi là STRING thì xóa int.TryParse đi, làm y như case 'C' nha)
                    if (int.TryParse(realIdString, out int bbIdInt))
                    {
                        var bb = db.ViolationRecords.FirstOrDefault(x => x.ViolationRecordId == bbIdInt);
                        if (bb != null)
                        {
                            // Lấy LawId của biên bản chui sang bảng Luật để tìm Tên
                            var luatCuaBB = db.TrafficLaws.FirstOrDefault(l => l.LawId == bb.LawId);
                            string tenLuat = luatCuaBB != null ? luatCuaBB.LawName : "Lỗi không xác định";
                            return $"Biên bản lỗi: {tenLuat}";
                        }
                    }
                    return "Biên bản (Đã xóa)";

                case 'P':
                    // 💡 Bảng Phản ánh: Giả sử ComplaintId là INT
                    if (int.TryParse(realIdString, out int paIdInt))
                    {
                        var pa = db.Complaints.FirstOrDefault(x => x.ComplaintId == paIdInt);
                        return pa != null ? $"Phản ánh: {pa.Title}" : "Phản ánh (Đã xóa)";
                    }
                    return "Mã phản ánh bị lỗi định dạng";

                case 'G':
                    // 💡 Bảng Bằng lái: LicenseNumber thường là STRING (để giữ số 0 ở đầu)
                    var bang = db.DrivingLicenses.FirstOrDefault(x => x.LicenseNumber == realIdString);
                    return bang != null ? $"Bằng lái xe: {bang.LicenseNumber}" : "Bằng lái xe (Đã xóa)";

                default:
                    return GetTargetDisplayName(targetId);
            }
        }
    }
}