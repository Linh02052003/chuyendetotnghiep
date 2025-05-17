using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var loaiList = db.Loais.ToList(); // Lấy tất cả các loại sách
            ViewBag.LoaiList = loaiList;
            var sachList = db.Saches.ToList(); // Lấy tất cả sách
            return View(sachList);
        }

        public ActionResult TimKiem(string skey)
        {
            // Truyền danh sách loại sách vào ViewBag để hiển thị danh mục (nếu cần)
            ViewBag.LoaiList = db.Loais.ToList();
            ViewBag.SearchKey = skey; // Lưu từ khóa tìm kiếm để hiển thị lại

            // Nếu skey rỗng hoặc null, trả về danh sách rỗng hoặc thông báo
            if (string.IsNullOrWhiteSpace(skey))
            {
                return View(new List<banSach.Models.Sach>()); // Trả về view TimKiem với danh sách rỗng
            }

            // Tìm kiếm sách theo tên hoặc mô tả
            var searchResults = db.Saches
                .Where(s => s.TenSach.Contains(skey) || (s.MoTa != null && s.MoTa.Contains(skey)))
                .ToList();

            // Trả về view TimKiem với danh sách sách tìm được
            return View(searchResults);
        }

        public ActionResult GioiThieu()
        {
            return View();
        }
        public ActionResult ChinhSachBanHang()
        {
            return View();
        }
        public ActionResult ChinhSachBaoMat()
        {
            return View();
        }
    }
}