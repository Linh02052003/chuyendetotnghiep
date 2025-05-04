using banSach.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class DangkyController : BaseController
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Register
        public ActionResult Index()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                // Lấy giá trị ConfirmPassword từ form
                string confirmPassword = Request.Form["ConfirmPassword"];

                // Kiểm tra xác nhận mật khẩu
                if (khachHang.MatKhau != confirmPassword)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                    return View(khachHang);
                }

                // Kiểm tra điều kiện mật khẩu mạnh
                if (khachHang.MatKhau.Length < 8 || !khachHang.MatKhau.Any(char.IsUpper) || !khachHang.MatKhau.Any(char.IsLower) || !khachHang.MatKhau.Any(char.IsDigit))
                {
                    TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và chữ số.";
                    return View(khachHang);
                }

                // Kiểm tra email hoặc số điện thoại đã tồn tại
                if (db.KhachHangs.Any(kh => kh.Email == khachHang.Email || kh.SoDienThoai == khachHang.SoDienThoai))
                {
                    TempData["Error"] = "Email hoặc số điện thoại đã được sử dụng.";
                    return View(khachHang);
                }

                // Tạo mã khách hàng tự động (KH001, KH002,...)
                var lastCustomer = db.KhachHangs.OrderByDescending(kh => kh.MaKH).FirstOrDefault();
                int newId = lastCustomer == null ? 1 : int.Parse(lastCustomer.MaKH.Replace("KH", "")) + 1;
                khachHang.MaKH = $"KH{newId:D3}";

                // Lưu vào cơ sở dữ liệu
                db.KhachHangs.Add(khachHang);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Index", "Dangnhap");
            }

            // Nếu ModelState không hợp lệ, trả lại view với thông tin đã nhập
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin đăng ký.";
            return View(khachHang);
        }
    }
}