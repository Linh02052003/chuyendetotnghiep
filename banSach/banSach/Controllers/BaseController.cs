using banSach.Models;
using System.Linq;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class BaseController : Controller
    {
        protected QLBanSachEntities db = new QLBanSachEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            TinhTongSoLuong();
        }

        private void TinhTongSoLuong()
        {
            var maKH = HttpContext.Session["MaKH"]?.ToString();
            if (!string.IsNullOrEmpty(maKH))
            {
                var gioHang = db.GioHangs.Include("ChiTietGioHang")
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
    }

}
