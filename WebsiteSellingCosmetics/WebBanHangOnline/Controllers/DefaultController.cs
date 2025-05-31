using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class DefaultController : Controller
    {
        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {   
            base.OnActionExecuting(filterContext);

            var userId = User.Identity.GetUserId();
            var user = UserManager.FindById(userId); 

            if (user != null && !user.IsActive)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Lockout.cshtml"
                };
            }
        }
    }
}