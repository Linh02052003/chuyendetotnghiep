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
using System.Web.UI;
using PagedList;


namespace bansach.Areas.Admin.Controllers
{
    public class ChucVusController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();

        // GET: Admin/ChucVus
        public async Task<ActionResult> Index(int ?page)
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
            int pageSize = 5; // số lượng mục trên mỗi trang
            int pageNumber = (page ?? 1); // trang hiện tại (mặc định là 1)

            var danhSach = db.ChucVus.OrderBy(cv => cv.MaCV).ToPagedList(pageNumber, pageSize);
            return View(danhSach);
        }

        // GET: Admin/ChucVus/Details/5
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
            ChucVu chucVu = await db.ChucVus.FindAsync(id);
            if (chucVu == null)
            {
                return HttpNotFound();
            }
            return View(chucVu);
        }

        // GET: Admin/ChucVus/Create
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

        // POST: Admin/ChucVus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var lastCV = db.ChucVus.OrderByDescending(c => c.MaCV).FirstOrDefault();
                    string newMaCV = "CV001";
                    if (lastCV != null)
                    {
                        string lastMa = lastCV.MaCV.Replace("CV", "");
                        int so = int.Parse(lastMa) + 1;
                        newMaCV = "CV" + so.ToString("D3");
                    }

                    chucVu.MaCV = newMaCV;
                    db.ChucVus.Add(chucVu);
                    await db.SaveChangesAsync();

                    // Thông báo thành công
                    TempData["SuccessMessage"] = "Chức vụ đã được thêm thành công!";
                }
                catch (Exception ex)
                {
                    // Thông báo thất bại
                    TempData["ErrorMessage"] = "Lỗi khi thêm chức vụ: " + ex.Message;
                }
                return RedirectToAction("Index");
            }
            return View(chucVu);
        }

        // GET: Admin/ChucVus/Edit/5
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

            ChucVu chucVu = await db.ChucVus.FindAsync(id);

            if (chucVu == null)
            {
                TempData["ErrorMessage"] = "Chức vụ không tồn tại!";
                return RedirectToAction("Index");
            }

            return View(chucVu);
        }
        // POST: Admin/ChucVus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MaCV,TenCV")] ChucVu chucVu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(chucVu).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["Message"] = "Chỉnh sửa chức vụ thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi chỉnh sửa chức vụ: " + ex.Message;
                }

                return RedirectToAction("Index");
            }
            return View(chucVu);
        }


        // GET: Admin/ChucVus/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ChucVu chucVu = await db.ChucVus.FindAsync(id);
            if (chucVu == null)
            {
                return HttpNotFound();
            }

            return View(chucVu);
        }

        // POST: Admin/ChucVus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            try
            {
                ChucVu chucVu = await db.ChucVus.FindAsync(id);
                if (chucVu == null)
                {
                    TempData["ErrorMessage"] = "Chức vụ không tồn tại!";
                    return RedirectToAction("Index");
                }

                db.ChucVus.Remove(chucVu);
                await db.SaveChangesAsync();

                // Thông báo xóa thành công
                TempData["SuccessMessage"] = "Chức vụ đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                // Thông báo thất bại
                TempData["ErrorMessage"] = "Lỗi khi xóa chức vụ: " + ex.Message;
            }

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
