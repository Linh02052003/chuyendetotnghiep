using banSach.Models;
using banSach.Helper;
using System;
using System.Linq;
using System.Web.Mvc;
using banSach.Other;
using System.Configuration;
using System.Collections.Generic;

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
                .Include("ChiTietGioHangs.Sach")
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

        public ActionResult DatHang(string PhuongThucThanhToan)
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

            if (PhuongThucThanhToan == "1") // Thanh toán tiền mặt
            {
                // Original logic: Process order immediately
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

                db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
                db.GioHangs.Remove(gioHang);
                db.SaveChanges();

                SendMail sendMail = new SendMail();
                sendMail.SendMailFunction(khachHang.Email, "Xác nhận đơn hàng từ Cửa hàng sách HT STORE", emailBody);

                TempData["Success"] = "Đặt hàng thành công!";
                Session["cart"] = null;
                Session["tongTienMua"] = null;

                return RedirectToAction("Index", "Home");
            }
            else if (PhuongThucThanhToan == "2") // Chuyển khoản (VNPay)
            {
                // Store order details in session without new models
                var orderDetails = new Dictionary<string, object>
                {
                    { "MaKH", maKH },
                    { "HoTen", khachHang.HoTen },
                    { "SoDienThoai", khachHang.SoDienThoai },
                    { "Email", khachHang.Email },
                    { "DiaChi", khachHang.DiaChi },
                    { "TotalAmount", gioHang.ChiTietGioHangs.Sum(item => (item.SoLuong ?? 0) * (item.DonGia ?? 0)) }
                };

                var cartItems = gioHang.ChiTietGioHangs.Select(item => new Dictionary<string, object>
                {
                    { "MaSach", item.MaSach },
                    { "SoLuong", item.SoLuong },
                    { "DonGia", item.DonGia }
                }).ToList();

                orderDetails["CartItems"] = cartItems;

                // Store in session
                Session["PendingOrder"] = orderDetails;

                // VNPay payment setup
                string url = ConfigurationManager.AppSettings["Url"];
                string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
                string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

                PayLib pay = new PayLib();
                pay.AddRequestData("vnp_Version", "2.1.0");
                pay.AddRequestData("vnp_Command", "pay");
                pay.AddRequestData("vnp_TmnCode", tmnCode);
                pay.AddRequestData("vnp_Amount", (Convert.ToDecimal(orderDetails["TotalAmount"]) * 100).ToString("F0"));
                pay.AddRequestData("vnp_BankCode", "");
                pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", "VND");
                pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
                pay.AddRequestData("vnp_Locale", "vn");
                pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang cho KH {maKH}");
                pay.AddRequestData("vnp_OrderType", "other");
                pay.AddRequestData("vnp_ReturnUrl", returnUrl);
                pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

                string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
                return Redirect(paymentUrl);
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

                if (checkSignature && vnp_ResponseCode == "00")
                {
                    // Payment successful, process the order
                    var orderDetails = Session["PendingOrder"] as Dictionary<string, object>;
                    if (orderDetails == null)
                    {
                        ViewBag.Message = "Không tìm thấy thông tin đơn hàng.";
                        return View();
                    }

                    // Extract customer details
                    string maKH = orderDetails["MaKH"]?.ToString();
                    string hoTen = orderDetails["HoTen"]?.ToString();
                    string soDienThoai = orderDetails["SoDienThoai"]?.ToString();
                    string email = orderDetails["Email"]?.ToString();
                    string diaChi = orderDetails["DiaChi"]?.ToString();
                    var cartItems = orderDetails["CartItems"] as List<Dictionary<string, object>>;

                    if (string.IsNullOrEmpty(maKH) || cartItems == null || !cartItems.Any())
                    {
                        ViewBag.Message = "Thông tin đơn hàng không hợp lệ.";
                        return View();
                    }

                    // Create order
                    var donHang = new DonDatHang
                    {
                        MaDonHang = Guid.NewGuid().ToString(),
                        HoTen = hoTen,
                        SoDienThoai = soDienThoai,
                        Email = email,
                        DiaChi = diaChi,
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

                    foreach (var item in cartItems)
                    {
                        string maSach = item["MaSach"]?.ToString();
                        int? soLuong = item["SoLuong"] as int?;
                        decimal? donGia = item["DonGia"] as decimal?;

                        var sach = db.Saches.Find(maSach);
                        if (sach != null)
                        {
                            var thanhTien = (soLuong ?? 0) * (donGia ?? 0);
                            tongTien += thanhTien;

                            emailBody += $"<tr>" +
                                         $"<td>{sach.TenSach}</td>" +
                                         $"<td style='text-align:center;'>{soLuong}</td>" +
                                         $"<td style='text-align:right;'>{donGia:N0}₫</td>" +
                                         $"<td style='text-align:right;'>{thanhTien:N0}₫</td>" +
                                         "</tr>";

                            sach.SoLuongTon -= soLuong ?? 0;
                        }

                        var chiTiet = new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaSach = maSach,
                            SoLuong = soLuong,
                            DonGia = donGia
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

                    // Clear cart
                    var gioHang = db.GioHangs
                                   .Include("ChiTietGioHangs")
                                   .FirstOrDefault(g => g.MaKH == maKH);
                    if (gioHang != null)
                    {
                        db.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
                        db.GioHangs.Remove(gioHang);
                    }

                    db.SaveChanges();

                    // Send email
                    SendMail sendMail = new SendMail();
                    sendMail.SendMailFunction(email, "Xác nhận đơn hàng từ Cửa hàng sách HT STORE", emailBody);

                    // Clear session
                    Session["PendingOrder"] = null;
                    Session["cart"] = null;
                    Session["tongTienMua"] = null;

                    ViewBag.Message = $"Thanh toán thành công hóa đơn {orderId} | Mã giao dịch: {vnpayTranId}";
                    TempData["Success"] = "Đặt hàng thành công!";
                }
                else
                {
                    // Payment failed
                    ViewBag.Message = checkSignature
                        ? $"Có lỗi xảy ra trong quá trình xử lý hóa đơn {orderId} | Mã giao dịch: {vnpayTranId} | Mã lỗi: {vnp_ResponseCode}"
                        : "Có lỗi xảy ra trong quá trình xử lý";
                    TempData["Error"] = ViewBag.Message;
                }
            }
            else
            {
                ViewBag.Message = "Không có thông tin thanh toán.";
                TempData["Error"] = ViewBag.Message;
            }

            return View();
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