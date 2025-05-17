using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using banSach.Helper;
using banSach.Models;
using Newtonsoft.Json;
using PagedList;

namespace banSach.Areas.Admin.Controllers
{
    public class DonDatHangsController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        public ActionResult Invoice(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonDatHang donDatHang = db.DonDatHangs.Include(d => d.ChiTietDonHangs.Select(c => c.Sach)).FirstOrDefault(d => d.MaDonHang == id);
            if (donDatHang == null)
            {
                return HttpNotFound();
            }
            return View(donDatHang);
        }
        public ActionResult CapNhatTrangThai(string maDonHang, string trangThai)
        {
            var donHang = db.DonDatHangs.Find(maDonHang);
            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index");
            }

            donHang.TrangThai = trangThai;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";

            // Gửi email nếu trạng thái là "Đang giao"
            if (trangThai == "Đang giao hàng")
            {
                decimal tongTien = 0;
                string emailBody = $"<div style='color: black;'>" +
                                    $"<p>Chào {donHang.HoTen},</p>" +
                                    $"<p>Đơn hàng của bạn (Mã đơn: <strong>{donHang.MaDonHang}</strong>) hiện đang được vận chuyển.</p>" +
                                    $"<p>Chúng tôi sẽ giao hàng đến bạn trong thời gian sớm nhất. Hãy chú ý điện thoại của bạn.</p>" +
                                    "<p>Chi tiết đơn hàng:</p>" +
                                    "<table border='1' cellspacing='0' cellpadding='5' style='border-collapse: collapse; width: 100%;'>" +
                                    "<thead><tr>" +
                                    "<th style='text-align:left;'>Tên sách</th>" +
                                    "<th>Số lượng</th>" +
                                    "<th>Đơn giá</th>" +
                                    "<th>Thành tiền</th>" +
                                    "</tr></thead><tbody>";

                foreach (var item in donHang.ChiTietDonHangs)
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
                }

                emailBody += $"<tr>" +
                             $"<td colspan='3' style='text-align:right; font-weight:bold;'>Tổng cộng:</td>" +
                             $"<td style='text-align:right; font-weight:bold;'>{tongTien:N0}₫</td>" +
                             $"</tr></tbody></table>" +
                             "<p><strong>Cảm ơn quý khách đã mua hàng tại cửa hàng Sách Trực Tuyến!</strong></p>" +
                             "<p>Hotline hỗ trợ: <strong>+060 (800) 801-582</strong></p>" +
                             "<p><em>Trân trọng,</em><br/>HT STORE</p>" +
                             "</div>";

                // Gửi email
                try
                {
                    SendMail sendMail = new SendMail();
                    sendMail.SendMailFunction(donHang.Email, "Thông báo về trạng thái đơn hàng: Đang giao hàng", emailBody);
                    TempData["SuccessMessage"] = "Đã gửi email xác nhận về trạng thái đơn hàng!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Đã có lỗi khi gửi email: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }
        // GET: Admin/DonDatHangs
        public ActionResult Index(int ?page)
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
            var donDatHangs = db.DonDatHangs.Include(d => d.ChiTietDonHangs).ToList();
            int pageSize = 5; // số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1); // trang hiện tại (mặc định là 1)

            var danhSach = db.DonDatHangs.OrderBy(hd => hd.MaDonHang).ToPagedList(pageNumber, pageSize);
            return View(danhSach);
        }


        // GET: Admin/DonDatHangs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DonDatHang donDatHang = db.DonDatHangs.Find(id);
            if (donDatHang == null)
            {
                return HttpNotFound();
            }
            return View(donDatHang);
        }

        // GET: Admin/DonDatHangs/Create
        public ActionResult Create()
        {
            // Fetch books with Status = 1
            var sachList = db.Saches
                .Where(s => s.Status == 1)
                .Select(s => new { s.MaSach, s.TenSach, s.GiaBan })
                .ToList();

            if (!sachList.Any())
            {
                System.Diagnostics.Debug.WriteLine("No books found with Status = 1");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Found {sachList.Count} books with Status = 1");
                foreach (var sach in sachList)
                {
                    System.Diagnostics.Debug.WriteLine($"Book: {sach.TenSach}, Price: {sach.GiaBan}");
                }
            }

            ViewBag.SachListJson = JsonConvert.SerializeObject(sachList, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return View();
        }

        // POST: Admin/DonDatHangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DonDatHang model)
        {
            if (model.ChiTietDonHangs == null || !model.ChiTietDonHangs.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một cuốn sách vào đơn hàng.");
            }

            if (ModelState.IsValid)
            {
                var donHang = new DonDatHang
                {
                    MaDonHang = "DH" + DateTime.Now.Ticks,
                    HoTen = model.HoTen,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    DiaChi = model.DiaChi,
                    NgayDat = DateTime.Now,
                    PhuongThucThanhToan = model.PhuongThucThanhToan,
                    TrangThai = string.IsNullOrEmpty(model.TrangThai) ? "Đã thanh toán" : model.TrangThai
                };

                db.DonDatHangs.Add(donHang);

                if (model.ChiTietDonHangs != null)
                {
                    foreach (var item in model.ChiTietDonHangs)
                    {
                        var chiTiet = new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaSach = item.MaSach,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        };
                        db.ChiTietDonHangs.Add(chiTiet);
                    }
                }

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi khi lưu đơn hàng. Vui lòng thử lại.");
                }
            }

            ViewBag.SachListJson = JsonConvert.SerializeObject(
                db.Saches.Where(s => s.Status == 1).Select(s => new { s.MaSach, s.TenSach, s.GiaBan }).ToList(),
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
            );
            return View(model);
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
