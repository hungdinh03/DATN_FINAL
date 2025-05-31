using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class AdvController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Adv
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AdvCarousel()
        {
            var items = db.Advs.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).ToList();
            return PartialView("_AdvCarousel", items);
        }
    }
}