using banSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace banSach.Controllers
{
    public class ModuleController : Controller
    {
        private QLBanSachEntities db = new QLBanSachEntities();
        // GET: Module
        public ActionResult Menu()
        {
            var categories = db.Loais.Where(l => l.Status == 1).ToList();
            return PartialView("Menu", categories);
        }
    }
}