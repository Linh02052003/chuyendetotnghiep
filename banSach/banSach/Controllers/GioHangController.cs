using banSach.Models;
using banSach.Helper;
using System;
using System.Linq;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class GioHangController : BaseController
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: GioHang
        public ActionResult Index()
        {
            var maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "Dangnhap");
            }

            // Fetch or create the cart for the customer
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaGioHang = Guid.NewGuid().ToString(),
                    MaKH = maKH,
                    NgayTao = DateTime.Now
                };
                db.GioHangs.Add(gioHang);
                db.SaveChanges();
            }

            // Eager load the cart items and their associated books
            gioHang = db.GioHangs
                .Include("ChiTietGioHangs.Sach") // Include navigation properties
                .FirstOrDefault(g => g.MaKH == maKH);

            return View(gioHang);
        }

        // GET: GioHang/ThemGiohang
        public ActionResult ThemGiohang(string iMasach, string strURL)
        {
            var maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sách giỏ hàng.";
                return RedirectToAction("Index", "Dangnhap");
            }

            // Find the book
            var sach = db.Saches.Find(iMasach);
            if (sach == null || sach.Status != 1 || sach.SoLuongTon <= 0)
            {
                TempData["Error"] = "Sách không tồn tại hoặc đã hết hàng.";
                return Redirect(strURL);
            }

            // Fetch or create the cart for the customer
            var gioHang = db.GioHangs.FirstOrDefault(g => g.MaKH == maKH);
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaGioHang = Guid.NewGuid().ToString(),
                    MaKH = maKH,
                    NgayTao = DateTime.Now
                };
                db.GioHangs.Add(gioHang);
                db.SaveChanges();
            }

            // Check if the book is already in the cart
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaSach == iMasach);
            if (chiTiet == null)
            {
                // Add new item to cart
                chiTiet = new ChiTietGioHang
                {
                    MaChiTiet = Guid.NewGuid().ToString(),
                    MaGioHang = gioHang.MaGioHang,
                    MaSach = iMasach,
                    SoLuong = 1,
                    DonGia = sach.GiaBan
                };
                db.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                // Increase quantity if book is already in cart
                chiTiet.SoLuong = (chiTiet.SoLuong ?? 0) + 1;
            }

            db.SaveChanges();
            TempData["Success"] = "Đã thêm sách vào giỏ hàng.";
            return Redirect(strURL);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(string maGioHang, string maSach, int soLuong)
        {
            try
            {
                var chiTiet = db.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == maGioHang && ct.MaSach == maSach);

                if (chiTiet == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

                var sach = db.Saches.Find(maSach);
                if (sach == null)
                    return Json(new { success = false, message = "Không tìm thấy sách." });

                if (soLuong <= 0)
                {
                    db.ChiTietGioHangs.Remove(chiTiet);
                }
                else
                {
                    if (soLuong > sach.SoLuongTon)
                        return Json(new { success = false, message = "Số lượng vượt quá tồn kho." });

                    chiTiet.SoLuong = soLuong;
                    db.Entry(chiTiet).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();

                decimal newSubtotal = (soLuong <= 0) ? 0 : soLuong * (chiTiet.DonGia ?? 0);
                decimal newTotal = db.ChiTietGioHangs
                    .Where(ct => ct.MaGioHang == maGioHang)
                    .Sum(ct => (ct.SoLuong ?? 0) * (ct.DonGia ?? 0));

                return Json(new
                {
                    success = true,
                    subtotal = newSubtotal,
                    total = newTotal,
                    formattedSubtotal = String.Format("{0:N0}", newSubtotal),
                    formattedTotal = String.Format("{0:N0}", newTotal)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: GioHang/RemoveItem
        public ActionResult RemoveItem(string maGioHang, string maSach)
        {
            var chiTiet = db.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHang == maGioHang && ct.MaSach == maSach);
            if (chiTiet != null)
            {
                db.ChiTietGioHangs.Remove(chiTiet);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private void TinhTongSoLuong()
        {
            var maKH = Session["MaKH"]?.ToString();
            if (!string.IsNullOrEmpty(maKH))
            {
                var gioHang = db.GioHangs.Include("ChiTietGioHangs")
                                         .FirstOrDefault(g => g.MaKH == maKH);

                if (gioHang != null && gioHang.ChiTietGioHangs != null)
                {
                    ViewBag.Tongsoluong = gioHang.ChiTietGioHangs.Sum(ct => ct.SoLuong ?? 0);
                }
                else
                {
                    ViewBag.Tongsoluong = 0;
                }
            }
            else
            {
                ViewBag.Tongsoluong = 0;
            }
        }


        public ActionResult Checkout()
        {
            var maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Index", "Dangnhap");
            }

            var khachHang = db.KhachHangs.Find(maKH);
            var gioHang = db.GioHangs.Include("ChiTietGioHangs.Sach").FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            ViewBag.KhachHang = khachHang;
            return View(gioHang.ChiTietGioHangs.ToList());
        }

        public ActionResult DatHang()
        {
            var maKH = Session["MaKH"]?.ToString();
            if (string.IsNullOrEmpty(maKH))
            {
                return RedirectToAction("Index", "Dangnhap");
            }

            var gioHang = db.GioHangs
                            .Include("ChiTietGioHangs")
                            .FirstOrDefault(g => g.MaKH == maKH);

            if (gioHang == null || !gioHang.ChiTietGioHangs.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            var khachHang = db.KhachHangs.Find(maKH);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Tạo đơn đặt hàng
            var donHang = new DonDatHang
            {
                MaDonHang = Guid.NewGuid().ToString(),
                HoTen = khachHang.HoTen,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi,
                NgayDat = DateTime.Now,
                TrangThai = "Chờ xác nhận"
            };
            db.DonDatHangs.Add(donHang);

            decimal tongTien = 0;
            string emailBody = $"<div style='color: black;'>" +
                $"<p>Chào {donHang.HoTen},</p>" +
                $"<p>Bạn đã đặt hàng thành công vào lúc {donHang.NgayDat:HH:mm:ss dd/MM/yyyy}.</p>" +
                $"<p>Mã đơn hàng của bạn là: <strong>{donHang.MaDonHang}</strong></p>" +
                "<p>Chi tiết đơn hàng:</p>" +
                "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
                "<thead><tr>" +
                "<th style='text-align:left;'>Tên sách</th>" +
                "<th>Số lượng</th>" +
                "<th>Đơn giá</th>" +
                "<th>Thành tiền</th>" +
                "</tr></thead><tbody>";

            foreach (var item in gioHang.ChiTietGioHangs)
            {
                var sach = db.Saches.Find(item.MaSach);
                if (sach != null)
                {
                    var thanhTien = (item.SoLuong ?? 0) * (item.DonGia ?? 0);
                    tongTien += thanhTien;

                    emailBody += $"<tr>" +
                                 $"<td>{sach.TenSach}</td>" +
                                 $"<td style='text-align:center;'>{item.SoLuong}</td>" +
                                 $"<td style='text-align:right;'>{item.DonGia:N0}₫</td>" +
                                 $"<td style='text-align:right;'>{thanhTien:N0}₫</td>" +
                                 "</tr>";

                    // Trừ tồn kho
                    sach.SoLuongTon -= item.SoLuong ?? 0;
                }

                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia
                };
                db.ChiTietDonHangs.Add(chiTiet);
            }

            emailBody += $"<tr>" +
                         $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
                         $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
                         $"</tr></tbody></table>" +
                         "<p><strong>Cảm ơn quý khách đã mua hàng tại cửa hàng Sách Trực Tuyến!</strong></p>" +
                         "<p>Hotline hỗ trợ: <strong>+060 (800) 801-582</strong></p>" +
                         "<p><em>Trân trọng,</em><br/>HT STORE</p>" +
                         "</div>";

            // Xóa giỏ hàng
            db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
            db.GioHangs.Remove(gioHang);

            db.SaveChanges();

            // Gửi email
            SendMail sendMail = new SendMail();
            sendMail.SendMailFunction(khachHang.Email, "Xác nhận đơn hàng từ Cửa hàng sách HT STORE", emailBody);

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Index", "GioHang");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}