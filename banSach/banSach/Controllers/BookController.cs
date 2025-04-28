using banSach.Models;
using banSach.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class BookController : BaseController
    {
        private QLBanSachEntities db = new QLBanSachEntities();
        // GET: Book
        public ActionResult Index(String id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch the category
            var category = db.Loais.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Fetch active books in the category (Status == 1)
            var books = db.Saches.Where(s => s.MaLoai == id && s.Status == 1).ToList();

            // Pass the category name to the view for display
            ViewBag.Category = category;

            return View(books);
        }
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var book = db.Saches.Find(id);
            if (book == null || book.Status != 1)
            {
                return HttpNotFound();
            }

            // Fetch authors and their roles via VietSach
            var authors = (from vs in db.VietSaches
                           join tg in db.TacGias on vs.MaTG equals tg.MaTG
                           where vs.MaSach == id && tg.Status == 1
                           select new SachChonViewModel
                           {
                               MaSach = vs.MaSach,
                               TenSach = vs.Sach.TenSach,
                               VaiTro = vs.VaiTro,
                               TenTG = tg.TenTG
                           }).ToList();

            // Log the results for debugging
            System.Diagnostics.Debug.WriteLine($"Book MaSach: {id}, Author Count: {authors.Count}");
            foreach (var author in authors)
            {
                System.Diagnostics.Debug.WriteLine($"Author: {author.TenTG}, Role: {author.VaiTro}");
            }

            ViewBag.Authors = authors;
            return View(book);
        }
    }
}