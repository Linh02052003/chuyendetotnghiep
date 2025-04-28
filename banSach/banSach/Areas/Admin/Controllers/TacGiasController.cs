using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using banSach.Models;
using banSach.ViewModels;

namespace banSach.Areas.Admin.Controllers
{
    public class TacGiasController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/TacGias
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
            return View(db.TacGias.ToList());
        }

        // GET: Admin/TacGias/Details/5
        // GET: Admin/TacGias/Details/5
        public ActionResult Details(string id)
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tacGia = db.TacGias.Find(id);
            if (tacGia == null)
            {
                return HttpNotFound();
            }

            var dsSach = db.Saches.ToList();
            var dsViet = db.VietSaches.Where(v => v.MaTG == id).ToList();

            var viewModel = new TacGiaModel
            {
                MaTG = tacGia.MaTG,  // Chắc chắn rằng MaTG được gán vào model
                TenTG = tacGia.TenTG,
                DiaChi = tacGia.DiaChi,
                TieuSu = tacGia.TieuSu,
                DienThoai = tacGia.DienThoai,
                Saches = dsSach.Select(s => new SachChonViewModel
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    DuocChon = dsViet.Any(v => v.MaSach == s.MaSach),
                    VaiTro = dsViet.FirstOrDefault(v => v.MaSach == s.MaSach)?.VaiTro
                }).ToList()
            };

            return View(viewModel);
        }


        // GET: Admin/TacGias/Create
        public ActionResult Create()
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
            var allSachs = db.Saches.ToList();

            var model = new TacGiaModel
            {
                Saches = allSachs.Select(s => new SachChonViewModel
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    DuocChon = false,
                    VaiTro = ""
                }).ToList()
            };

            return View(model);
        }

        // POST: Admin/TacGias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TacGiaModel model)
        {
            if (ModelState.IsValid)
            {
                // Tạo mã tác giả mới
                var last = db.TacGias.OrderByDescending(t => t.MaTG).FirstOrDefault();
                string newMaTG = "TG001";
                if (last != null)
                {
                    int so = int.Parse(last.MaTG.Substring(2)) + 1;
                    newMaTG = "TG" + so.ToString("D3");
                }

                var tacGia = new TacGia
                {
                    MaTG = newMaTG,
                    TenTG = model.TenTG,
                    DiaChi = model.DiaChi,
                    TieuSu = model.TieuSu,
                    DienThoai = model.DienThoai,
                    Status = model.Status = 1
                };

                db.TacGias.Add(tacGia);

                // Gán sách được chọn
                foreach (var sach in model.Saches.Where(s => s.DuocChon))
                {
                    db.VietSaches.Add(new VietSach
                    {
                        MaTG = newMaTG,
                        MaSach = sach.MaSach,
                        VaiTro = sach.VaiTro
                    });
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm tác giả và sách thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/TacGias/Edit/5
        // GET: Admin/TacGias/Edit/5
        public ActionResult Edit(string id)
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tacGia = db.TacGias.Find(id);
            if (tacGia == null)
            {
                return HttpNotFound();
            }

            var sachs = db.Saches.ToList();
            var sachesChon = sachs.Select(s => new SachChonViewModel
            {
                MaSach = s.MaSach,
                TenSach = s.TenSach,
                DuocChon = tacGia.VietSaches.Any(ts => ts.MaSach == s.MaSach),
                VaiTro = tacGia.VietSaches.FirstOrDefault(ts => ts.MaSach == s.MaSach)?.VaiTro
            }).ToList();

            var model = new TacGiaModel
            {
                MaTG = tacGia.MaTG,  // Lưu lại MaTG
                TenTG = tacGia.TenTG,
                DiaChi = tacGia.DiaChi,
                TieuSu = tacGia.TieuSu,
                DienThoai = tacGia.DienThoai,
                Status = tacGia.Status = 1,
                Saches = sachesChon
            };

            return View(model);
        }


        // POST: Admin/TacGias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TacGiaModel model)
        {
            if (ModelState.IsValid)
            {
                // Gán lại MaTG từ model sang đối tượng tacGia
                var tacGia = db.TacGias.Find(model.MaTG);
                if (tacGia != null)
                {
                    tacGia.TenTG = model.TenTG;
                    tacGia.DiaChi = model.DiaChi;
                    tacGia.TieuSu = model.TieuSu;
                    tacGia.DienThoai = model.DienThoai;
                    tacGia.Status = model.Status = 1;

                    // Cập nhật các sách đã viết
                    // Xử lý liên kết sách và vai trò (bảng VietSach)
                    foreach (var sach in model.Saches.Where(s => s.DuocChon))
                    {
                        var vietSach = db.VietSaches.FirstOrDefault(vs => vs.MaTG == tacGia.MaTG && vs.MaSach == sach.MaSach);
                        if (vietSach != null)
                        {
                            vietSach.VaiTro = sach.VaiTro;
                        }
                        else
                        {
                            db.VietSaches.Add(new VietSach
                            {
                                MaTG = tacGia.MaTG,
                                MaSach = sach.MaSach,
                                VaiTro = sach.VaiTro
                            });
                        }
                    }

                    db.Entry(tacGia).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                    return RedirectToAction("Index");
                }
                return HttpNotFound();
            }
            return View(model);
        }


        // GET: Admin/TacGias/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TacGia tacGia = db.TacGias.Find(id);  // Tìm tác giả theo id
            if (tacGia == null)
            {
                return HttpNotFound();  // Nếu không tìm thấy tác giả
            }

            return View(tacGia);  // Trả về view để xác nhận xóa
        }

        // POST: Admin/TacGias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            // Tìm tác giả theo id
            TacGia tacGia = db.TacGias.Find(id);
            if (tacGia == null)
            {
                return HttpNotFound();  // Nếu không tìm thấy tác giả, trả về lỗi
            }

            // Xóa các bản ghi liên quan trong bảng VietSach
            var vietSaches = db.VietSaches.Where(v => v.MaTG == id).ToList();
            db.VietSaches.RemoveRange(vietSaches);  // Xóa tất cả các bản ghi liên quan đến tác giả

            // Sau khi xóa các bản ghi trong bảng VietSach, xóa tác giả
            db.TacGias.Remove(tacGia);
            db.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu

            TempData["SuccessMessage"] = "Xóa tác giả thành công!";  // Thông báo thành công
            return RedirectToAction("Index");  // Chuyển hướng về trang danh sách tác giả
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
