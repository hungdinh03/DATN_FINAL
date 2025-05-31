using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebBanHangOnline.Models;

using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System.Collections.Generic;
using ClientApp.Attributes;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    [CustomAuthorize("~/Account/LoginRegister")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

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

        //
        // GET: /Account/LoginRegister
        [AllowAnonymous]
        public ActionResult LoginRegister()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var model = new LoginRegisterViewModel
            {
                Login = new LoginViewModel(),
                Register = new RegisterViewModel()
            };

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            return View(model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, errors = ReturnErrors() });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ReturnErrors() });
            }

            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Case sai username
                return Json(new { success = false, errors = new List<string> { "Tên đăng nhập hoặc mật khẩu của bạn không đúng! Vui lòng thử lại!" } });
            }

            var roles = await UserManager.GetRolesAsync(user.Id);
            if (!roles.Contains("Admin") && !user.EmailConfirmed)
            {
                return Json(new { success = false, errors = new List<string> { "Tài khoản của bạn chưa được xác thực email!" } });
            }              

            if (!user.IsActive)
            {
                return Json(new { success = false, errors = new List<string> { "Tài khoản của bạn bị khóa!" } });
            }

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:     
                    return Json(new { success = true });                
                case SignInStatus.LockedOut: // Muốn dùng case này thì chỉnh shouldLockout: true
                    return Json(new { success = false, errors = new List<string> { "Bạn đã đăng nhập thất bại 5 lần! Vui lòng thử lại sau 5 phút!" } });
                case SignInStatus.RequiresVerification:
                    return Json(new { success = false, errors = new List<string> { "Xác minh tài khoản của bạn!" } });
                case SignInStatus.Failure: // Case sai password
                    return Json(new { success = false, errors = new List<string> { "Tên đăng nhập hoặc mật khẩu của bạn không đúng! Vui lòng thử lại!" } });
                default:
                    return Json(new { success = false, errors = ReturnErrors() });
            }
        }

        //
        // POST: /Account/Logoff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logoff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("LoginRegister", "Account");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, errors = ReturnErrors() });
            }

            if (ModelState.IsValid)
            {
                var existingUser = await UserManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    return Json(new { success = false, error = "Tên người dùng đã được sử dụng. Vui lòng chọn tên khác." });
                }

                var existingEmail = await UserManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    return Json(new { success = false, error = "Địa chỉ email này đã được sử dụng. Vui lòng nhập một địa chỉ email khác." });
                }
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Address = model.Address,
                    Email = model.Email,
                    CreatedDate = DateTime.Now,
                    IsActive = false
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Customer");
                    db.Carts.Add(new Cart
                    {
                        UserId = user.Id
                    });
                    db.SaveChanges();
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, Token = token }, protocol: Request.Url.Scheme);
                    WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Xác thực email", "Truy cập <a href='" + callbackUrl + "'>liên kết này</a> để đăng nhập.", model.Email);
                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return Json(new { success = true });
                }
                AddErrors(result);
            }
            return Json(new { success = false });
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                await UserManager.ConfirmEmailAsync(userId, token);
                var user = await UserManager.FindByIdAsync(userId);
                user.IsActive = true;
                await UserManager.UpdateAsync(user);
                ViewBag.ValidLink = true;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ValidLink = false;
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return PartialView("_ForgotPassword");
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return Json(new { success = false, errors = new List<string> { "Không có tài khoản có email này!" } });
                }

                if (!user.EmailConfirmed)
                {
                    return Json(new { success = false, errors = new List<string> { "Tài khoản của bạn chưa được xác thực email!" } });
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { UserId = user.Id, Token = token }, protocol: Request.Url.Scheme);
                WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Quên mật khẩu", "Truy cập <a href='" + callbackUrl + "'>liên kết này</a> để đặt lại mật khẩu.", model.Email);
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return Json(new { success = true });
            }

            // If we got this far, something failed, redisplay form
            return Json(new { success = false, errors = ReturnErrors() });
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return PartialView("_ForgotPasswordConfirmation");
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string token)
        {
            if (userId == null || token == null || !(await UserManager.VerifyUserTokenAsync(userId, "ResetPassword", token)))
            {
                ViewBag.ValidLink = false;
                return View();
            }
            ViewBag.ValidLink = true;
            var model = new ResetPasswordViewModel { UserId = userId, Token = token };
            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false });
            }
            var result = await UserManager.ResetPasswordAsync(model.UserId, model.Token, model.Password);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Link xác thực không chính xác!" });
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return PartialView("_ResetPassword");
        }

        public ActionResult Lockout()
        {
            return View();
        }

        public new async Task<ActionResult> Profile()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public new async Task<ActionResult> Profile(ApplicationUser req)
        {
            var user = await UserManager.FindByEmailAsync(req.Email);
            user.FullName = req.FullName;
            user.Phone = req.Phone;
            user.Address = req.Address;
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public async Task<JsonResult> GetUserInfo()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                var userInfo = new
                {
                    user.FullName,
                    user.Phone,
                    user.Address,
                    user.Email
                };
                return Json(userInfo, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        public ActionResult OrderHistory(int page = 1)
        {
            if (User.Identity.IsAuthenticated)
            {
                int pageSize = 10;
                var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindByName(User.Identity.Name);
                var items = db.Orders.Where(x => x.CustomerId == user.Id).OrderByDescending(x => x.CreatedDate).ToList();

                var pagedItems = items.ToPagedList(page, pageSize);
                ViewBag.PageSize = pageSize;
                ViewBag.Page = page;
                return View(pagedItems);
            }
            return View();
        }

        public ActionResult OrderDetail(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        [HttpPost]
        //public ActionResult CancelOrder(string code)
        //{
        //    var order = db.Orders.SingleOrDefault(x => x.Code == code);
        //    if (order == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if ((DateTime.Now - order.CreatedDate).TotalHours < 24)
        //    {
        //        order.Status = 3;
        //        db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}

        public ActionResult CancelOrder(string code)
        {
            var order = db.Orders.SingleOrDefault(x => x.Code == code);
            if (order == null)
            {
                return Json(new { message = "Order not found", success = false });
            }

            // Kiểm tra điều kiện hủy đơn (ví dụ: trong vòng 24 giờ kể từ khi tạo)
            if ((DateTime.Now - order.CreatedDate).TotalHours >= 24)
            {
                return Json(new { message = "Order can only be canceled within 24 hours of creation.", success = false });
            }

            var oldStatus = order.Status; // Lưu trạng thái cũ (có thể không dùng đến nếu logic luôn hoàn trả)
                                          // Đặt trạng thái đơn hàng là "Đã hủy" (ví dụ: status = 3)
            order.Status = 3;
            db.Entry(order).Property(x => x.Status).IsModified = true;

            // Lấy danh sách chi tiết đơn hàng
            var orderDetails = db.OrderDetails.Where(od => od.OrderId == order.Id).ToList();

            // Duyệt qua từng sản phẩm trong đơn hàng để hoàn trả Quantity và giảm SoldQuantity
            foreach (var item in orderDetails)
            {
                var product = db.Products.Find(item.ProductId);
                if (product != null)
                {
                    // Hoàn trả số lượng sản phẩm vào kho
                    product.Quantity += item.Quantity;
                    db.Entry(product).Property(p => p.Quantity).IsModified = true;

                    // Giảm số lượng đã bán
                    product.SoldQuantity -= item.Quantity;
                    if (product.SoldQuantity < 0)
                        product.SoldQuantity = 0; // Đảm bảo SoldQuantity không âm
                    db.Entry(product).Property(p => p.SoldQuantity).IsModified = true;
                }
            }

            try
            {
                db.SaveChanges();
                // Có thể gọi SyncSoldQuantity() nếu cần đồng bộ hóa ở đây
                return Json(new { message = "Order canceled successfully.", success = true });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây
                return Json(new { message = "Error canceling order: " + ex.Message, success = false });
            }
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private List<string> ReturnErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}