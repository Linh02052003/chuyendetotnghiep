using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using banSach.Models;
using System.IO;
using PagedList;
using System.Web.UI;

namespace bansach.Areas.Admin.Controllers
{
    public class SachsController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        public ActionResult ToggleStatus(string id)
        {
            var sach = db.Saches.Find(id);
            if (sach != null)
            {
                sach.Status = sach.Status == 1 ? 0 : 1;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        // GET: Admin/Saches
        public async Task<ActionResult> Index(int? page)
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
            var sach = db.Saches.Include(s => s.Loai).Include(s => s.NhaXuatBan);
            int pageSize = 5; // số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1); // trang hiện tại (mặc định là 1)

            var danhSach = db.Saches.OrderBy(s => s.MaSach).ToPagedList(pageNumber, pageSize);
            return View(danhSach);
        }

        // GET: Admin/Saches/Details/5
        public async Task<ActionResult> Details(string id)
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
            Sach sach = await db.Saches.FindAsync(id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            return View(sach);
        }

        // GET: Admin/Saches/Create
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
            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai");
            ViewBag.MaNXB = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB");
            return View();
        }

        // POST: Admin/Saches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TenSach,GiaBan,MoTa,MaNXB,NgayNhapHang,SoLuongTon,MaLoai,Status")] Sach sach, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã sách tự động
                var lastSach = db.Saches.OrderByDescending(s => s.MaSach).FirstOrDefault();
                string newMaSach = "S001";

                if (lastSach != null)
                {
                    string lastMa = lastSach.MaSach.Replace("S", "");
                    int so = int.Parse(lastMa) + 1;
                    newMaSach = "S" + so.ToString("D3");
                }

                sach.MaSach = newMaSach;

                // Xử lý upload hình nếu có
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/sach/"), fileName);
                    HinhAnh.SaveAs(path);
                    sach.Hinh = fileName;
                }
                sach.Status = 1;
                db.Saches.Add(sach);
                await db.SaveChangesAsync();
                TempData["Message"] = "Thêm sách thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", sach.MaLoai);
            ViewBag.MaNXB = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }

        // GET: Admin/Saches/Edit/5
        public async Task<ActionResult> Edit(string id)
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
            Sach sach = await db.Saches.FindAsync(id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", sach.MaLoai);
            ViewBag.MaNXB = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }

        // POST: Admin/Saches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MaSach,TenSach,Hinh,GiaBan,MoTa,MaNXB,NgayNhapHang,SoLuongTon,MaLoai")] Sach sach, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload hình nếu có
                if (HinhAnh != null && HinhAnh.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(HinhAnh.FileName);
                    var path = Path.Combine(Server.MapPath("~/img/sach/"), fileName);
                    HinhAnh.SaveAs(path);
                    sach.Hinh = fileName;
                }
                sach.Status = 1;
                db.Entry(sach).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["Message"] = "Cập nhật sách thành công!";
                return RedirectToAction("Index");
            }
            
            ViewBag.MaLoai = new SelectList(db.Loais, "MaLoai", "TenLoai", sach.MaLoai);
            ViewBag.MaNXB = new SelectList(db.NhaXuatBans, "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }

        // GET: Admin/Saches/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sach sach = await db.Saches.FindAsync(id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            return View(sach);
        }

        // POST: Admin/Saches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Sach sach = await db.Saches.FindAsync(id);
            db.Saches.Remove(sach);
            await db.SaveChangesAsync();
            TempData["Message"] = "Xóa sách thành công!";
            return RedirectToAction("Index");
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
