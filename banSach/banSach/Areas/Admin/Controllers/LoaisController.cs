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

namespace bansach.Areas.Admin.Controllers
{
    public class LoaisController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        public ActionResult ToggleStatus(string id)
        {
            var loai = db.Loais.Find(id);
            if (loai != null)
            {
                loai.Status = loai.Status == 1 ? 0 : 1;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/Loais
        public async Task<ActionResult> Index()
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
            return View(await db.Loais.ToListAsync());
        }

        // GET: Admin/Loais/Details/5
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
            Loai loai = await db.Loais.FindAsync(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // GET: Admin/Loais/Create
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
            return View();
        }

        // POST: Admin/Loais/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TenLoai,Status")] Loai loai)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã tự động
                var lastLoai = db.Loais.OrderByDescending(l => l.MaLoai).FirstOrDefault();
                string newMaLoai = "L001";

                if (lastLoai != null)
                {
                    string lastMa = lastLoai.MaLoai.Replace("L", "");
                    int so = int.Parse(lastMa) + 1;
                    newMaLoai = "L" + so.ToString("D3");
                }

                loai.MaLoai = newMaLoai;
                loai.Status = 1;
                db.Loais.Add(loai);
                await db.SaveChangesAsync();
                TempData["Message"] = "Thêm loại sách thành công!";
                return RedirectToAction("Index");
            }

            return View(loai);
        }

        // GET: Admin/Loais/Edit/5
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
            Loai loai = await db.Loais.FindAsync(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // POST: Admin/Loais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MaLoai,TenLoai,Status")] Loai loai)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loai).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["Message"] = "Cập nhật loại sách thành công!";
                return RedirectToAction("Index");
            }
            loai.Status = 1;
            return View(loai);
        }

        // GET: Admin/Loais/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Loai loai = await db.Loais.FindAsync(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // POST: Admin/Loais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Loai loai = await db.Loais.FindAsync(id);
            db.Loais.Remove(loai);
            await db.SaveChangesAsync();
            TempData["Message"] = "Xóa loại sách thành công!";
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
