using banSach.Models;
using banSach.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class DangnhapController : BaseController
    {
        // GET: Login
        private QLBanSachEntities db = new QLBanSachEntities();
        public ActionResult Index()
        {
            return View();
        }

        // POST: Dangnhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection form)
        {
            string username = form["Username"];
            string password = form["Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập số điện thoại hoặc email và mật khẩu!";
                return View();
            }

            var khachHang = db.KhachHangs
                .FirstOrDefault(kh => (kh.SoDienThoai == username || kh.Email == username) && kh.MatKhau == password);

            if (khachHang != null)
            {
                Session["KhachHang"] = khachHang;
                Session["MaKH"] = khachHang.MaKH;
                Session["HoTen"] = khachHang.HoTen;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Số điện thoại, email hoặc mật khẩu không đúng!";
                return View();
            }
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            var user = db.KhachHangs.FirstOrDefault(k => k.Email == Email);
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return View();
            }

            // Tạo token ngẫu nhiên
            string token = Guid.NewGuid().ToString();
            Session["ResetToken_" + token] = user.MaKH;

            // Tạo link đặt lại mật khẩu
            string resetLink = Url.Action("ResetPassword", "Dangnhap", new { token = token }, protocol: Request.Url.Scheme);

            string subject = "Yêu cầu đặt lại mật khẩu";
            string body = $@"
<p>Xin chào <b>{user.HoTen}</b>,</p>
<p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng nhấn vào nút bên dưới để thiết lập lại:</p>
<p style='text-align: center; margin: 30px 0;'>
    <a href='{resetLink}' target='_blank' 
       style='display: inline-block; padding: 12px 24px; background-color: #ff6f00; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
        Đặt Lại Mật Khẩu
    </a>
</p>
<p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email.</p>";


            SendMail sendMail = new SendMail();
            bool result = sendMail.SendMailFunction(user.Email, subject, body);

            
            return View();
        }

        public ActionResult ResetPassword(string token)
        {
            if (Session["ResetToken_" + token] == null)
            {
                return Content("Link không hợp lệ hoặc đã hết hạn.");
            }
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string token, string newPassword)
        {
            // Kiểm tra xem token có hợp lệ không
            var maKH = Session["ResetToken_" + token];
            if (maKH == null)
            {
                TempData["Error"] = "Token không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ResetPassword", new { token = token }); // Quay lại trang reset password với thông báo lỗi
            }

            // Tìm người dùng từ cơ sở dữ liệu
            var user = db.KhachHangs.Find(maKH);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại!";
                return RedirectToAction("ResetPassword", new { token = token }); // Quay lại trang reset password với thông báo lỗi
            }

            // Kiểm tra tính mạnh mẽ của mật khẩu
            if (newPassword.Length < 8 || !newPassword.Any(char.IsUpper) || !newPassword.Any(char.IsLower) || !newPassword.Any(char.IsDigit))
            {
                TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và chữ số.";
                return RedirectToAction("ResetPassword", new { token = token }); // Quay lại trang reset password với thông báo lỗi
            }

            // Lưu mật khẩu mới vào cơ sở dữ liệu (không mã hóa)
            user.MatKhau = newPassword; // Lưu mật khẩu trực tiếp

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Xóa session và thông báo thành công
            Session.Remove("ResetToken_" + token);
            TempData["Success"] = "Đổi mật khẩu thành công!";

            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("Index", "Dangnhap");
        }




        // GET: Dangnhap/Logout
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Index", "Home");
        }
    }
}