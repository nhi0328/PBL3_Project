namespace PBL3.Models;

public class ViolationSearchResult
{
    public int RecordId { get; set; }
    public int Status { get; set; }
    public string Loi { get; set; } = string.Empty;
    public string ThoiGian { get; set; } = string.Empty;
    public string DiaDiem { get; set; } = string.Empty;
    public string TrangThai => IsProcessed ? "Đã xử lý" : "Chưa xử lý";
    public bool IsProcessed => Status == 1;
}
