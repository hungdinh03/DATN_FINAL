using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: News
        public ActionResult Index(int page = 1)
        {
            var pageSize = 5;
            IEnumerable<News> items = db.News.OrderByDescending(x => x.CreatedDate).Where(x => x.IsActive);
            items = items.ToPagedList(page, pageSize);
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            return View(items);
        }

        public ActionResult Detail(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }

        public ActionResult NewsHome()
        {
            var items = db.News.OrderByDescending(x => x.CreatedDate).Where(x => x.IsHome).Take(3).ToList();
            return PartialView("_NewsHome", items);
        }
    }
}