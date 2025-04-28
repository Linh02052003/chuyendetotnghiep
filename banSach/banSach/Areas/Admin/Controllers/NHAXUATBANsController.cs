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
    public class NhaXuatBansController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/NhaXuatBans
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
            return View(await db.NhaXuatBans.ToListAsync());
        }

        // GET: Admin/NhaXuatBans/Details/5
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
            NhaXuatBan nhaXuatBan = await db.NhaXuatBans.FindAsync(id);
            if (nhaXuatBan == null)
            {
                return HttpNotFound();
            }
            return View(nhaXuatBan);
        }

        // GET: Admin/NhaXuatBans/Create
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

        // POST: Admin/NhaXuatBans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TenNXB,DiaChi")] NhaXuatBan nhaXuatBan)
        {
            if (ModelState.IsValid)
            {
                // Tạo mã NXB tự động
                var lastNXB = db.NhaXuatBans.OrderByDescending(n => n.MaNXB).FirstOrDefault();
                string newMaNXB = "NXB001";

                if (lastNXB != null)
                {
                    string lastMa = lastNXB.MaNXB.Replace("NXB", "");
                    int so = int.Parse(lastMa) + 1;
                    newMaNXB = "NXB" + so.ToString("D3");
                }

                nhaXuatBan.MaNXB = newMaNXB;

                db.NhaXuatBans.Add(nhaXuatBan);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                return RedirectToAction("Index");
            }

            return View(nhaXuatBan);
        }


        // GET: Admin/NhaXuatBans/Edit/5
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
            NhaXuatBan nhaXuatBan = await db.NhaXuatBans.FindAsync(id);
            if (nhaXuatBan == null)
            {
                return HttpNotFound();
            }
            return View(nhaXuatBan);
        }

        // POST: Admin/NhaXuatBans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MaNXB,TenNXB,DiaChi")] NhaXuatBan nhaXuatBan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nhaXuatBan).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                return RedirectToAction("Index");
            }
            return View(nhaXuatBan);
        }

        // GET: Admin/NhaXuatBans/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NhaXuatBan nhaXuatBan = await db.NhaXuatBans.FindAsync(id);
            if (nhaXuatBan == null)
            {
                return HttpNotFound();
            }
            return View(nhaXuatBan);
        }

        // POST: Admin/NhaXuatBans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            NhaXuatBan nhaXuatBan = await db.NhaXuatBans.FindAsync(id);
            db.NhaXuatBans.Remove(nhaXuatBan);
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
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
