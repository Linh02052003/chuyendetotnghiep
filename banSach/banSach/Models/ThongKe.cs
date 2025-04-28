using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace banSach.Models
{
    public class ThongKe
    {
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string LoaiThongKe { get; set; } // "ngay" hoặc "thang"
        public int TongSoDonHang { get; set; }
        public decimal TongDoanhThu { get; set; }
        public List<ChiTietThongKe> ChiTiet { get; set; }
    }

    public class ChiTietThongKe
    {
        public string KhoangThoiGian { get; set; } // Định dạng: "dd/MM/yyyy" hoặc "MM/yyyy"
        public int SoDonHang { get; set; }
        public decimal DoanhThu { get; set; }
    }
}