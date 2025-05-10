using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace banSach.ViewModels
{
    public class TacGiaModel
    {
        public string MaTG { get; set; }
        public string TenTG { get; set; }
        public string DiaChi { get; set; }
        public string TieuSu { get; set; }
        public string DienThoai { get; set; }
        public Nullable<int> Status { get; set; }

        // Danh sách sách được chọn kèm vai trò
        public List<SachChonViewModel> Saches { get; set; }
    }

    public class SachChonViewModel
    {
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public bool DuocChon { get; set; }
        public string VaiTro { get; set; }
        public string TenTG { get; set; }
        public string TieuSu { get; set; }

    }
}
