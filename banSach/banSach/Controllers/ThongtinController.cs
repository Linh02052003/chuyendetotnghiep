using banSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace banSach.Controllers
{
    public class ThongtinController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Thongtin
        public ActionResult Index()
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem thông tin cá nhân!";
                return RedirectToAction("Index", "Dangnhap");
            }

            var maKH = Session["MaKH"]?.ToString();
            var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKH == maKH);
            if (khachHang == null)
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Thông tin tài khoản không tồn tại!";
                return RedirectToAction("Index", "Dangnhap");
            }

            return View(khachHang);
        }

        // POST: Thongtin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhachHang model)
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để chỉnh sửa thông tin!";
                return RedirectToAction("Index", "Dangnhap");
            }

            if (ModelState.IsValid)
            {
                var maKH = Session["MaKH"]?.ToString();
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKH == maKH);
                if (khachHang == null)
                {
                    Session.Clear();
                    TempData["ErrorMessage"] = "Thông tin tài khoản không tồn tại!";
                    return RedirectToAction("Index", "Dangnhap");
                }

                khachHang.HoTen = model.HoTen;
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.Email = model.Email;
                khachHang.DiaChi = model.DiaChi;
                khachHang.NgaySinh = model.NgaySinh;

                db.SaveChanges();

                Session["HoTen"] = khachHang.HoTen;
                Session["KhachHang"] = khachHang;

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại!";
            return View("Index", model);
        }

        // GET: Thongtin/DoiMatKhau
        public ActionResult DoiMatKhau()
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu!";
                return RedirectToAction("Index", "Dangnhap");
            }

            return View();
        }

        // POST: Thongtin/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đổi mật khẩu!";
                return RedirectToAction("Index", "Dangnhap");
            }

            var maKH = Session["MaKH"]?.ToString();
            var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.MaKH == maKH);
            if (khachHang == null)
            {
                Session.Clear();
                TempData["ErrorMessage"] = "Thông tin tài khoản không tồn tại!";
                return RedirectToAction("Index", "Dangnhap");
            }

            if (khachHang.MatKhau != oldPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không đúng!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận không khớp!";
                return View();
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View();
            }

            khachHang.MatKhau = newPassword;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }

        // GET: Thongtin/DonHang
        public ActionResult DonHang()
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng!";
                return RedirectToAction("Index", "Dangnhap");
            }

            var hoTen = Session["HoTen"]?.ToString();
            var donHangs = db.DonDatHangs
                            .Include(dh => dh.ChiTietDonHangs) // Sử dụng lambda đúng cách
                            .Where(dh => dh.HoTen == hoTen)
                            .OrderByDescending(dh => dh.NgayDat)
                            .ToList();

            // Tạo Dictionary để lưu tổng tiền cho mỗi đơn hàng
            var tongTienDict = new Dictionary<string, decimal>();
            foreach (var dh in donHangs)
            {
                var tongTien = dh.ChiTietDonHangs
                                .Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));
                tongTienDict[dh.MaDonHang] = tongTien;
            }

            // Truyền danh sách đơn hàng và Dictionary tổng tiền sang view
            ViewBag.TongTienDict = tongTienDict;
            return View(donHangs);
        }

        public ActionResult HuyDonHang(string id)
        {
            var donHang = db.DonDatHangs.Find(id);
            if (donHang != null && donHang.TrangThai == "Chờ xác nhận")
            {
                donHang.TrangThai = "Đã hủy"; // Update status to "Cancelled"
                db.SaveChanges();
                TempData["Message"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng.";
            }
            return RedirectToAction("DonHang");
        }
        // GET: Thongtin/ChiTietDonHang
        public ActionResult ChiTietDonHang(string id)
        {
            if (Session["KhachHang"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng!";
                return RedirectToAction("Index", "Dangnhap");
            }

            // Lấy danh sách chi tiết đơn hàng
            var chiTiet = db.ChiTietDonHangs
                            .Include(ct => ct.Sach) // Bao gồm thông tin sách
                            .Where(ct => ct.MaDonHang == id)
                            .ToList();

            // Lấy thông tin đơn hàng từ DonDatHang
            var donHang = db.DonDatHangs
                           .FirstOrDefault(dh => dh.MaDonHang == id);

            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("DonHang");
            }

            // Tính tổng tiền
            decimal tongTien = chiTiet.Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));

            // Truyền thông tin qua ViewBag
            ViewBag.MaDonHang = id;
            ViewBag.NgayDat = donHang.NgayDat;
            ViewBag.TrangThai = donHang.TrangThai;
            ViewBag.TongTien = tongTien;

            return View(chiTiet);
        }
    }
}
