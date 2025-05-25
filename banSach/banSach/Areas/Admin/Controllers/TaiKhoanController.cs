using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using banSach.Models;

namespace banSach.Areas.Admin.Controllers
{
    public class TaiKhoanController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/TaiKhoan/Index
        public ActionResult Index()
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
            var nhanVien = Session["AdminUser"] as NhanVien;
            if (nhanVien == null) return RedirectToAction("Index", "Login", new { Area = "Admin" });
            return View(nhanVien);
        }

        // POST: Admin/TaiKhoan/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string matKhau, string matKhauMoi, string xacNhanMatKhau, string tenTK, string soDienThoai, string email)
        {
            var nhanVien = Session["AdminUser"] as NhanVien;
            if (nhanVien == null) return RedirectToAction("Index", "Login", new { Area = "Admin" });

            if (string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(matKhauMoi) || string.IsNullOrEmpty(xacNhanMatKhau) || string.IsNullOrEmpty(tenTK))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View("Index", nhanVien);
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận không khớp.");
                return View("Index", nhanVien);
            }

            if (nhanVien.MatKhau != matKhau)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không đúng.");
                return View("Index", nhanVien);
            }

            var dbNhanVien = db.NhanViens.Find(nhanVien.MaNhanVien);
            if (dbNhanVien == null) return HttpNotFound();

            dbNhanVien.MatKhau = matKhauMoi;
            dbNhanVien.TenTK = tenTK;
            dbNhanVien.SoDienThoai = soDienThoai;
            dbNhanVien.Email = email;

            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(tenTK, false);
            Session["AdminUser"] = dbNhanVien;

            TempData["Message"] = "Cập nhật thành công!";
            return RedirectToAction("Index");
        }

    }

}
