using ClientApp.Attributes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using PagedList;

namespace WebBanHangOnline.Controllers
{
    [CustomAuthorize("/Account/LoginRegister")]
    public class ReviewProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ReviewProduct
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Review(int productId)
        {
            ViewBag.ProductId = productId;
            var item = new ReviewProduct();
            if (User.Identity.IsAuthenticated)
            {
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(User.Identity.Name);
                if (user != null)
                {
                    item.Email = user.Email;
                    item.FullName = user.FullName;
                    item.UserName = user.UserName;
                }
                return PartialView("_Review", item);
            }
            return PartialView("_Review");
        }

        [AllowAnonymous]
        public ActionResult LoadReview(int productId, int? page)
        {
            var pageSize = 5; // Số lượng đánh giá mỗi trang
            var pageIndex = page ?? 1; // Trang hiện tại
            var reviews = db.ReviewProducts
                             .Where(x => x.ProductId == productId)
                             .OrderByDescending(x => x.CreatedDate)
                             .ToPagedList(pageIndex, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            ViewBag.ProductId = productId; // Truyền productId cho các yêu cầu phân trang

            return PartialView("_LoadReview", reviews);
        }

        [HttpPost]
        public ActionResult PostReview(ReviewProduct req)
        {
            if (ModelState.IsValid)
            {
                req.CreatedDate = DateTime.Now;
                db.ReviewProducts.Add(req);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public JsonResult GetAverageRating(int productId)
        {
            var reviews = db.ReviewProducts.Where(r => r.ProductId == productId).ToList();
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rate) : 0;
            var count = reviews.Count;

            return Json(new { average = averageRating, count }, JsonRequestBehavior.AllowGet);
        }

    }
}