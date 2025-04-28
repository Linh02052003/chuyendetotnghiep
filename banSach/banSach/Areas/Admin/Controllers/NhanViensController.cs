using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using banSach.Models;

namespace bansach.Areas.Admin.Controllers
{
    public class NhanViensController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/NhanViens
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
            return View(await db.NhanViens.Include(nv => nv.ChucVu).ToListAsync());
        }

        // GET: Admin/NhanViens/Details/5
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
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = await db.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // GET: Admin/NhanViens/Create
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
            SetViewBagChucVu();
            return View();
        }

        // POST: Admin/NhanViens/Create
        // POST: Admin/NhanViens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "HoTen,SoDienThoai,Email,TenTK,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã tự động
                var lastNV = db.NhanViens.OrderByDescending(nv => nv.MaNhanVien).FirstOrDefault();
                string newMaNV = "NV001";

                if (lastNV != null)
                {
                    string lastMa = lastNV.MaNhanVien.Replace("NV", "");
                    int so = int.Parse(lastMa) + 1;
                    newMaNV = "NV" + so.ToString("D3");
                }

                nhanVien.MaNhanVien = newMaNV;

                db.NhanViens.Add(nhanVien);
                await db.SaveChangesAsync();

                // Thông báo thành công
                TempData["Message"] = "Nhân viên đã được thêm thành công!";
                return RedirectToAction("Index");
            }

            SetViewBagChucVu(nhanVien.MaCV);
            return View(nhanVien);
        }


        // GET: Admin/NhanViens/Edit/5
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
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = await db.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            SetViewBagChucVu(nhanVien.MaCV);
            return View(nhanVien);
        }

        // POST: Admin/NhanViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MaNhanVien,HoTen,SoDienThoai,Email,TenTK,MatKhau,MaCV")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nhanVien).State = EntityState.Modified;
                await db.SaveChangesAsync();

                // Thông báo thành công
                TempData["Message"] = "Chỉnh sửa nhân viên thành công!";
                return RedirectToAction("Index");
            }

            SetViewBagChucVu(nhanVien.MaCV);
            return View(nhanVien);
        }


        // GET: Admin/NhanViens/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NhanVien nhanVien = await db.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                return HttpNotFound();
            }

            return View(nhanVien);
        }

        // POST: Admin/NhanViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            NhanVien nhanVien = await db.NhanViens.FindAsync(id);
            db.NhanViens.Remove(nhanVien);
            await db.SaveChangesAsync();

            // Thông báo thành công
            TempData["Message"] = "Nhân viên đã được xóa thành công!";
            return RedirectToAction("Index");
        }


        private void SetViewBagChucVu(string selectedMaCV = null)
        {
            ViewBag.MaCV = new SelectList(db.ChucVus, "MaCV", "TenCV", selectedMaCV);
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
