using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models;

[Table("PROVINCES")]
public class Province
{
    [Key]
    [Column("PROVINCE_ID")]
    public int ProvinceId { get; set; }

    [Column("PROVINCE_NAME")]
    public string ProvinceName { get; set; } = string.Empty;

    // --- CÁC MỐI QUAN HỆ LIÊN KẾT ---

    // 1 Tỉnh thì có nhiều Phường/Xã (Mối quan hệ 1-Nhiều)
    public virtual ICollection<Ward> Wards { get; set; }

    public Province()
    {
        // Khởi tạo list trống để tránh lỗi NullReferenceException
        Wards = new HashSet<Ward>();
    }
}