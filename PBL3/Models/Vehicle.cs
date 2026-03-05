using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    [Table("VEHICLES")] // Khớp với tên bảng trong SQL
    public partial class Vehicle
    {
        [Key] // <--- THÊM DÒNG NÀY VÀO ĐÂY
        [Column("LICENSE_PLATE")] // Khớp với tên cột trong SQL
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string OwnerId { get; set; }

        public virtual User Owner { get; set; }
    }
}
