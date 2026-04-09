using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PBL3.Models;

public static class ViolationLookupService
{
    // --- 1. Hàm lấy chi tiết một vụ vi phạm (Dùng cho Page17, Page22...) ---
    public static ViolationDetailResult? GetViolationDetail(int recordId)
    {
        using var db = new TrafficSafetyDBContext(); // Đảm bảo gọi đúng DBContext

        // Lấy hồ sơ vi phạm + Tự động NỐI BẢNG lấy Luật, Chi tiết luật và Loại xe
        var baseRecord = db.ViolationRecords
            .Include(r => r.Law)                  // Nối bảng TRAFFIC_LAWS
            .Include(r => r.Category)             // Nối bảng CATEGORIES
            .FirstOrDefault(r => r.ViolationRecordId == recordId); // Thay Stt bằng ViolationRecordId

        if (baseRecord == null) return null;

        // Lấy tất cả các lỗi vi phạm của cùng một xe tại cùng một thời điểm
        var relatedRecords = db.ViolationRecords
            .Include(r => r.Law)
            .Where(r => r.LicensePlate == baseRecord.LicensePlate
                     && r.ViolationDate == baseRecord.ViolationDate
                     && r.ViolationTime == baseRecord.ViolationTime
                     && r.Address == baseRecord.Address)
            .ToList();

        // Đảm bảo baseRecord luôn nằm trong danh sách nếu chưa có (trong trường hợp lỗi logic)
        if (!relatedRecords.Any(r => r.ViolationRecordId == baseRecord.ViolationRecordId))
        {
            relatedRecords.Insert(0, baseRecord);
        }

        string headerSubtitle = "Lỗi vi phạm: ";
        string fullFineRange = "";
        string fullPointsDeducted = "";
        string fullDescription = "";

        if (relatedRecords.Count == 1)
        {
            var record = relatedRecords[0];
            string fine = "Chưa có dữ liệu";
            string points = "Không trừ điểm/phạt bổ sung";

            if (record.LawId.HasValue && record.CategoryId.HasValue)
            {
                var d = db.TrafficLawDetails.FirstOrDefault(x => x.LawId == record.LawId && x.CategoryId == record.CategoryId);
                if (d != null)
                {
                    fine = d.FineAmount ?? "Chưa có dữ liệu";
                    points = (d.DemeritPoints != null && d.DemeritPoints > 0) ? $"{d.DemeritPoints} điểm" : "Không có";
                }
            }

            headerSubtitle += record.Law?.LawName ?? record.ViolationDescription ?? "Không rõ";
            fullFineRange = fine;
            fullPointsDeducted = points;
            fullDescription = record.ViolationDescription ?? record.Law?.LawName ?? "Không rõ";
        }
        else
        {
            for (int i = 0; i < relatedRecords.Count; i++)
            {
                var record = relatedRecords[i];
                string fine = "Chưa có dữ liệu";
                string points = "Không trừ điểm";

                if (record.LawId.HasValue && record.CategoryId.HasValue)
                {
                    var d = db.TrafficLawDetails.FirstOrDefault(x => x.LawId == record.LawId && x.CategoryId == record.CategoryId);
                    if (d != null)
                    {
                        fine = d.FineAmount ?? "Chưa có dữ liệu";
                        points = (d.DemeritPoints != null && d.DemeritPoints > 0) ? $"{d.DemeritPoints} điểm" : "Không có";
                    }
                }

                string linePrefix = relatedRecords.Count > 1 ? $"{i + 1}. " : "";
                string loiPrefix = relatedRecords.Count > 1 ? $"Lỗi {i + 1}: " : "";

                if (i > 0)
                {
                    headerSubtitle += "\n";
                    fullFineRange += "\n";
                    fullPointsDeducted += "\n";
                    fullDescription += " ";
                }

                string lawName = record.Law?.LawName ?? record.ViolationDescription ?? "Không rõ";
                headerSubtitle += (i == 0 ? $"{linePrefix}{lawName}" : $"                     {linePrefix}{lawName}");

                fullFineRange += $"{loiPrefix}{fine}";
                fullPointsDeducted += $"{loiPrefix}{points}";
                fullDescription += record.ViolationDescription ?? record.Law?.LawName ?? "Không rõ";
            }
        }

        // Đổ dữ liệu vào object Result để đẩy lên giao diện
        return new ViolationDetailResult
        {
            RecordId = baseRecord.ViolationRecordId,
            HeaderTitle = $"Phương tiện {baseRecord.LicensePlate}",
            HeaderSubtitle = headerSubtitle,

            // Lấy tên Loại xe từ bảng Categories (Ví dụ: "Xe máy", "Ô tô")
            VehicleType = baseRecord.Category?.CategoryName ?? "Không xác định",

            ViolationDate = baseRecord.ViolationDate?.ToString("dd/MM/yyyy") ?? "Không rõ",
            ViolationTime = baseRecord.ViolationTime?.ToString(@"hh\:mm") ?? "Không rõ",
            ViolationLocation = baseRecord.Address ?? "Không ghi nhận", // Thay DetailedLocation bằng Address

            // Lấy Tên lỗi từ bảng TrafficLaw, nếu không có thì lấy Description
            ViolationDescription = fullDescription,

            FineRange = fullFineRange,
            PaymentLocation = "Cổng Dịch vụ công Quốc gia / Kho bạc Nhà nước",
            PointsDeducted = fullPointsDeducted,

            IsProcessed = baseRecord.Status == 1,
            StatusText = baseRecord.StatusText, // Gọi thẳng thuộc tính StatusText đã tạo trong Model

            EvidenceImagePath = baseRecord.ImagePath ?? "", // Thay EvidenceImagePath bằng ImagePath
            EvidenceCaption = string.IsNullOrEmpty(baseRecord.ImagePath) ? "Không có hình ảnh ghi nhận" : "Hình ảnh trích xuất từ camera",

            LastUpdated = baseRecord.LastUpdate?.ToString("HH:mm dd/MM/yyyy") ?? "" // Thay LastUpdated bằng LastUpdate
        };
    }

    // --- 2. Hàm tìm kiếm danh sách vi phạm nhanh (Dùng cho Page10 của Khách) ---
    public static List<ViolationSearchResult> SearchViolations(string keyword)
    {
        using var db = new TrafficSafetyDBContext();

        // Include bảng Law để lấy được tên lỗi vi phạm
        var query = db.ViolationRecords.Include(r => r.Law).AsQueryable();

        // Nếu khách nhập biển số xe thì lọc ra
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keyToLower = keyword.Trim().ToLower();
            query = query.Where(r => r.LicensePlate != null && r.LicensePlate.ToLower().Contains(keyToLower));
        }

        // Sắp xếp ngày mới nhất lên đầu
        var records = query.OrderByDescending(r => r.ViolationDate).ToList();

        // Chuyển sang dạng Result để hiện lên DataGrid
        return records.Select(r => new ViolationSearchResult
        {
            RecordId = r.ViolationRecordId,
            Status = r.Status,
            // Ưu tiên hiện Tên Luật chính thức, nếu không có mới hiện Mô tả
            Loi = r.Law?.LawName ?? r.ViolationDescription ?? "Vi phạm giao thông",
            ThoiGian = r.ViolationDate?.ToString("dd/MM/yyyy") ?? "Không rõ",
            DiaDiem = r.Address ?? "Không rõ"
        }).ToList();
    }
}