using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemConfigController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Category
        public ActionResult Index()
        {
            var items = db.SystemConfigs.OrderBy(x => x.Position);
            ViewBag.MaxPosition = items.Count();
            return View(items);
        }

        public ActionResult Add()
        {
            int? maxPosition = db.SystemConfigs.Count();
            ViewBag.MaxPosition = maxPosition ?? 0;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(SystemConfig model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = User.Identity.Name;
                model.ModifiedBy = User.Identity.Name;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                var posList = db.SystemConfigs.OrderBy(x => x.Position).ToList();
                int prevPos = model.Position;
                foreach (var pos in posList)
                {
                    if (pos.Position == prevPos)
                    {
                        pos.Position++;
                        prevPos = pos.Position;
                    }
                }
                db.SystemConfigs.Add(model);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = true });
        }
        
        public ActionResult Edit(int id)
        {
            var item = db.SystemConfigs.Find(id);
            return Json(new { success = true,
                              id = item.Id,
                              title = item.Title,
                              pos = item.Position,
                              maxPos = db.SystemConfigs.Count()
                            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SystemConfig model)
        {
            
            if (ModelState.IsValid)
            {
                var item = db.SystemConfigs.SingleOrDefault(x => x.Id == model.Id);
                int prevPos = item.Position;

                item.Title = model.Title;
                item.Position = model.Position;
                item.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                item.ModifiedDate = DateTime.Now;
                item.ModifiedBy = User.Identity.Name;                
                db.SaveChanges();

                var configs = db.SystemConfigs.OrderBy(x => x.Position).ToList();
                for (int i = 0; i < configs.Count; i++)
                {
                    if (configs[i].Id == model.Id) continue;
                    if (configs[i].Id != model.Id && configs[i].Position == model.Position)
                    {
                        if (prevPos < model.Position) configs[i].Position--;
                        else configs[i].Position++;                        
                    }
                    else configs[i].Position = i + 1;
                }
                db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.SystemConfigs.Find(id);
            if (item != null)
            {
                var posList = db.SystemConfigs.OrderBy(x => x.Position).ToList();
                foreach (var pos in posList)
                {
                    if (pos.Position > item.Position)
                    {
                        pos.Position--;
                    }
                }
                db.SystemConfigs.Remove(item);                
                db.SaveChanges();                
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}