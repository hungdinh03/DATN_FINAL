using System;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Subscribe()
        { 
            return PartialView("_Subscribe");
        }

        [HttpPost]
        public ActionResult Subscribe(Subscribe req)
        {
            if (ModelState.IsValid)
            {
                var item = db.Subscribes.SingleOrDefault(s => s.Email.Equals(req.Email));
                if (item == null)
                {
                    db.Subscribes.Add(new Subscribe { Email = req.Email, CreatedDate = DateTime.Now });
                    db.SaveChanges();
                }
                return Json(true);           
            }
            return PartialView("_Subscribe");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}