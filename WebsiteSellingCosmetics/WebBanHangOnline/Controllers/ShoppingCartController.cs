using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using ClientApp.Attributes;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;
using System.Collections.Generic;
using System.Web;

namespace WebBanHangOnline.Controllers
{
    [CustomAuthorize("~/Account/LoginRegister")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Phương thức để lấy giỏ hàng hiện tại
        private ShoppingCart GetCurrentCart()
        {
            var userId = User.Identity.GetUserId();
            return ShoppingCart.LoadCart(userId, db);
        }

        // GET: ShoppingCart
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Index()
        {
            var cart = GetCurrentCart();
            ViewBag.CheckCart = cart.Items.Any(); // Kiểm tra xem giỏ hàng có bất kỳ sản phẩm nào không
            return View(cart.Items); // Truyền danh sách sản phẩm cho View
        }

        public ActionResult Cart()
        {
            var cart = GetCurrentCart();
            ViewBag.CheckCart = cart;
            return View(cart);
        }

        public ActionResult ItemCart()
        {
            var cart = GetCurrentCart();
            return PartialView("_ItemCart", cart.Items);
        }

        public ActionResult ShowCount()
        {
            var cart = GetCurrentCart();
            return Json(new { count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            // Nếu chưa đăng nhập thì chuyển đến trang đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { redirectToLogin = Url.Action("LoginRegister", "Account") });
            }
            var product = db.Products.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                var cart = GetCurrentCart();
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Title,
                    ProductImg = product.ProductImage.FirstOrDefault(x => x.IsDefault)?.Image,
                    Alias = product.Alias,
                    Price = (int)((product.PriceSale > 0) ? product.PriceSale : product.Price),
                    Quantity = quantity,
                    LeftQuantity = product.Quantity,
                    TotalPrice = (int)((product.PriceSale > 0) ? product.PriceSale : product.Price) * quantity,
                    CategoryName = product.ProductCategory.Title
                };
                cart.AddToCart(item, db);
                return Json(new { success = true, count = cart.Items.Count });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var cart = GetCurrentCart();
            cart.Remove(id, db);
            return Json(new { success = true, count = cart.Items.Count });
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            var cart = GetCurrentCart();
            cart.ClearAllCart(db);
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            var cart = GetCurrentCart();
            cart.UpdateItemCartQuantity(id, quantity, db);
            return Json(new { success = true });
        }

        public ActionResult UserCheckOut()
        {
            return PartialView("_UserCheckOut");
        }

        public ActionResult ItemCheckOut()
        {
            var cart = GetCurrentCart();
            var selectedProductIds = (List<int>)Session["SelectedProductIds"];
            if (selectedProductIds != null) 
            { 
                var selectedItems = cart.Items.Where(x => selectedProductIds.Contains(x.ProductId)).ToList();
                return PartialView("_ItemCheckOut", selectedItems);
            }            
            return PartialView("_ItemCheckOut");
        }

        [HttpPost]
        public ActionResult ItemCheckOut(List<int> selectedProductIds)
        {
            if (selectedProductIds != null)
            {
                Session["SelectedProductIds"] = selectedProductIds;
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Chưa chọn sản phẩm nào!" });
        }

        public ActionResult CheckOut()
        {
            var cart = GetCurrentCart();
            System.Diagnostics.Debug.WriteLine(Session["SelectedProductIds"]);
            ViewBag.CheckCart = cart;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            return View();
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cart = GetCurrentCart();
                var selectedProductIds = (List<int>)Session["SelectedProductIds"];
                if (cart != null && selectedProductIds != null)
                {
                    var selectedItems = cart.Items.Where(x => selectedProductIds.Contains(x.ProductId)).ToList();

                    Order order = new Order
                    {
                        Code = "DH" + new Random().Next(1000, 9999),
                        CustomerId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null,
                        CustomerName = model.CustomerName,
                        Phone = model.Phone,
                        Address = model.Address,
                        Email = model.Email,
                        TotalAmount = selectedItems.Sum(x => (x.Price * x.Quantity)),
                        TypePayment = model.TypePayment,
                        Status = 0,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = User.Identity.GetUserName()
                    };

                    selectedItems.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        ProductImg = x.ProductImg,
                        Alias = x.Alias,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        TotalPrice = x.TotalPrice,
                        CategoryName = x.CategoryName
                    }));                    

                    // Send email when order is successful
                    var strSanPham = "";
                    var thanhTien = decimal.Zero;
                    var tongTien = decimal.Zero;
                    foreach (var sp in selectedItems)
                    {
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + string.Format("{0:N0}", sp.TotalPrice) + "</td>";
                        strSanPham += "</tr>";
                        thanhTien += sp.Price * sp.Quantity;
                    }
                    tongTien = thanhTien;
                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", model.Email);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentCustomer = contentCustomer.Replace("{{HinhThuc}}", model.TypePayment == 1 ? "COD" : "Chuyển khoản");
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", string.Format("{0:N0}", thanhTien));
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", string.Format("{0:N0}", tongTien));                

                    if (model.TypePayment == 1)
                    {
                        db.Orders.Add(order);
                        db.SaveChanges();
                        WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentCustomer.ToString(), model.Email);
                        cart.ClearItemCart(selectedProductIds, db);
                        cart.UpdateProductQuantity(order, db);
                        Session["SelectedProductIds"] = null;
                        return Json(new { success = true , redirectUrl = "/ShoppingCart/CodReturn" });                     
                    } 
                    else
                    {
                        TempData["Order"] = order;
                        TempData["Email"] = model.Email;
                        TempData["MailContent"] = contentCustomer;
                        return Json(new { success = true, redirectUrl = UrlPayment(model.VnPayTypePayment, order) });
                    }                    
                }
                return Json(new { success = false, redirectUrl = "/gio-hang" });
            }
            return View(model);
        }

        public ActionResult CodReturn()
        {
            return View();
        }

        public ActionResult VnPayReturn()
        {
            if (Request.QueryString.Count > 0)
            {                
                var vnPayData = Request.QueryString;
                VnPayLibrary vnPay = new VnPayLibrary();
                foreach (string s in vnPayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnPay.AddResponseData(s, vnPayData[s]);
                    }
                }

                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
                string vnp_ResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnPay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                long vnp_Amount = Convert.ToInt64(vnPay.GetResponseData("vnp_Amount")) / 100;

                bool checkSignature = vnPay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature && vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {        
                    if (TempData["Order"] != null) 
                    {
                        var order = (Order)TempData["Order"];
                        var email = TempData["Email"].ToString();
                        var mailContent = TempData["MailContent"].ToString();
                        order.Status = 1;
                        db.Orders.Add(order);
                        db.SaveChanges();
                        WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, mailContent, email);

                        var cart = GetCurrentCart();
                        var selectedProductIds = (List<int>)Session["SelectedProductIds"];
                        cart.ClearItemCart(selectedProductIds, db);
                        cart.UpdateProductQuantity(order, db);
                        Session["SelectedProductIds"] = null;                      
                    }
                    // Successful transaction
                    ViewBag.Amount = vnp_Amount;
                    ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                }
                else
                {
                    // Failed transaction. Error code: vnp_ResponseCode
                    ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý. " + VnPayResponseCode.GetErrorCodes(vnp_ResponseCode);
                }
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult BuyNow(int id, int quantity = 1)
        {
            // Nếu chưa đăng nhập thì chuyển đến trang đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { redirectToLogin = Url.Action("LoginRegister", "Account") });
            }

            var product = db.Products.FirstOrDefault(x => x.Id == id);
            if (product != null)
            {
                var cart = GetCurrentCart();
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Title,
                    ProductImg = product.ProductImage.FirstOrDefault(x => x.IsDefault)?.Image,
                    Alias = product.Alias,
                    Price = (int)((product.PriceSale > 0) ? product.PriceSale : product.Price),
                    Quantity = quantity,
                    LeftQuantity = product.Quantity,
                    TotalPrice = (int)((product.PriceSale > 0) ? product.PriceSale : product.Price) * quantity,
                    CategoryName = product.ProductCategory.Title
                };

                cart.AddToCart(item, db);

                // Lưu ID sản phẩm vào Session để chỉ chọn sản phẩm này khi thanh toán
                List<int> selectedProductIds = new List<int> { product.Id };
                Session["SelectedProductIds"] = selectedProductIds;

                return Json(new { success = true, redirectUrl = "/ShoppingCart/CheckOut" });
            }
            return Json(new { success = false });
        }

        #region Thanh toán VNPay
        public string UrlPayment(int vnPayTypePayment, Order order)
        {
            // Get Config Info
            string vnp_ReturnUrl = ConfigurationManager.AppSettings["vnp_ReturnUrl"]; // URL returning transaction response
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; // VNPay check-out URL
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; // Terminal Code
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; // Secret Key

            // Build URL for VNPAY
            VnPayLibrary vnPay = new VnPayLibrary();
            var price = (long)order.TotalAmount * 100;
            vnPay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnPay.AddRequestData("vnp_Command", "pay");
            vnPay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            // Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán
            // là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnPay.AddRequestData("vnp_Amount", price.ToString()); 
            if (vnPayTypePayment == 1)
            {
                vnPay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (vnPayTypePayment == 2)
            {
                vnPay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (vnPayTypePayment == 3)
            {
                vnPay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            vnPay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnPay.AddRequestData("vnp_CurrCode", "VND");
            vnPay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnPay.AddRequestData("vnp_Locale", "vn");
            vnPay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Code);
            vnPay.AddRequestData("vnp_OrderType", "other"); // Default value: other
            vnPay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày
            vnPay.AddRequestData("vnp_TxnRef", order.Code);

            return vnPay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }
        #endregion
    }
}
