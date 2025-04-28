using System;
using System.Linq;
using System.Web.Mvc;
using banSach.Models;
using System.Collections.Generic;
using System.Globalization;

namespace banSach.Areas.Admin.Controllers
{
    public class ThongkeController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/Thongke
        public ActionResult Index(string loaiThongKe = "thang", DateTime? ngayBatDau = null, DateTime? ngayKetThuc = null)
        {
            if (Session["AdminUser"] == null)
            {
                return RedirectToAction("Index", "Login", new { area = "Admin" });
            }

            var user = Session["AdminUser"] as NhanVien;
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "Admin" });
            }

            ViewBag.HoTen = user.HoTen;
            // Mặc định: 30 ngày trước
            var ketThuc = ngayKetThuc ?? DateTime.Now;
            var batDau = ngayBatDau ?? ketThuc.AddDays(-30);

            // Kiểm tra loại thống kê
            if (loaiThongKe != "ngay" && loaiThongKe != "thang")
            {
                loaiThongKe = "thang";
            }

            // Lấy danh sách đơn hàng "Hoàn tất" hoặc "Đã thanh toán"
            var donHangs = db.DonDatHangs
                .Include("ChiTietDonHangs") // Sửa từ lambda sang string
                .Where(d =>
                    (d.TrangThai == "Hoàn tất" || d.TrangThai == "Đã thanh toán")
                    && d.NgayDat >= batDau && d.NgayDat <= ketThuc)
                .ToList();

            // Tạo model thống kê
            var model = new ThongKe
            {
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                LoaiThongKe = loaiThongKe,
                TongSoDonHang = donHangs.Count,
                TongDoanhThu = donHangs.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0))),
                ChiTiet = new List<ChiTietThongKe>()
            };

            // Nhóm theo ngày hoặc tháng
            if (loaiThongKe == "ngay")
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => o.NgayDat.Value.Date)
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = g.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => DateTime.ParseExact(r.KhoangThoiGian, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }
            else
            {
                model.ChiTiet = donHangs
                    .GroupBy(o => new { o.NgayDat.Value.Year, o.NgayDat.Value.Month })
                    .Select(g => new ChiTietThongKe
                    {
                        KhoangThoiGian = $"{g.Key.Month}/{g.Key.Year}",
                        SoDonHang = g.Count(),
                        DoanhThu = g.Sum(o => o.ChiTietDonHangs.Sum(c => (c.SoLuong ?? 0) * (c.DonGia ?? 0)))
                    })
                    .OrderBy(r => DateTime.ParseExact(r.KhoangThoiGian, "M/yyyy", CultureInfo.InvariantCulture))
                    .ToList();
            }

            return View(model);
        }
    }
}
