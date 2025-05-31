using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(string searchText, string fromDate, string toDate, int page = 1)
        {
            var pageSize = 10;           

            var items = db.Orders.OrderByDescending(x => x.CreatedDate).AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                items = items.Where(x => x.Code.Contains(searchText) ||
                                         x.CustomerName.Contains(searchText) ||
                                         x.Phone.Contains(searchText) ||
                                         x.CreatedBy.Contains(searchText));
            }

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);
                DateTime endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null);
                items = items.Where(x => DbFunctions.TruncateTime(x.CreatedDate) >= startDate && DbFunctions.TruncateTime(x.CreatedDate) <= endDate);
            }

            var pagedList = items.ToPagedList(page, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.SearchText = searchText;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(pagedList);
        }

        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        public ActionResult ItemsOrdered(int id)
        {
            var item = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView("_ItemsOrdered", item);
        }

        //[HttpPost]
        //public ActionResult UpdateStatus(int id, int status)
        //{
        //    var item = db.Orders.Find(id);
        //    if (item != null)
        //    {
        //        db.Orders.Attach(item);
        //        item.Status = status;
        //        db.Entry(item).Property(x => x.Status).IsModified = true;
        //        db.SaveChanges();
        //        return Json(new { message = "Success", success = true });
        //    }
        //    return Json(new { message = "Unsuccess", success = false });
        //}

        [HttpPost]
        public ActionResult UpdateStatus(int id, int status)
        {
            var order = db.Orders.Find(id);
            if (order == null)
                return Json(new { message = "Order not found", success = false });

            var oldStatus = order.Status;
            var currentCustomerId = order.CustomerId;
            order.Status = status;
            db.Entry(order).Property(x => x.Status).IsModified = true;

            // Lấy danh sách chi tiết đơn hàng
            var orderDetails = db.OrderDetails.Where(od => od.OrderId == id).ToList();

            // Nếu chuyển sang trạng thái "Đã giao"
            if (oldStatus != 2 && status == 2)
            {
                foreach (var item in orderDetails)
                {
                    var product = db.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.SoldQuantity += item.Quantity;
                        db.Entry(product).Property(p => p.SoldQuantity).IsModified = true;
                    }
                }
            }
            // Nếu chuyển trạng thái từ "Đã giao" sang trạng thái khác (ví dụ hủy)
            else if (oldStatus == 2 && status != 2)
            {
                foreach (var item in orderDetails)
                {
                    var product = db.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.SoldQuantity -= item.Quantity;
                        if (product.SoldQuantity < 0)
                            product.SoldQuantity = 0;
                        db.Entry(product).Property(p => p.SoldQuantity).IsModified = true;
                    }
                }
            }

            UpdateCustomerNameForCustomerOrders(currentCustomerId);

            try
            {
                db.SaveChanges();
                SyncSoldQuantity();
                return Json(new { message = "Success", success = true });
            }
            catch (Exception ex)
            {
                // Bạn có thể log lỗi ở đây (vd: logger.Error(ex))
                return Json(new { message = "Error: " + ex.Message, success = false });
            }
        }

        public void SyncSoldQuantity()
        {
            // 1. Lấy tất cả sản phẩm
            var products = db.Products.ToList();

            // 2. Lấy tất cả OrderDetails thuộc Orders đã giao (Status = 2)
            var paidOrderDetails = db.OrderDetails
                                     .Where(od => od.Order.Status == 2)
                                     .ToList();

            // 3. Duyệt từng product, tính tổng số đã bán
            foreach (var product in products)
            {
                int totalSold = paidOrderDetails
                                .Where(od => od.ProductId == product.Id)
                                .Sum(od => od.Quantity);

                product.SoldQuantity = totalSold;

                db.Entry(product).Property(p => p.SoldQuantity).IsModified = true;
            }

            db.SaveChanges();
        }

        private void UpdateCustomerNameForCustomerOrders(string customerId)
        {
            // 1. Lấy FullName mới nhất từ bảng ApplicationUser (bảng user của bạn)
            var user = db.Users.Find(customerId); // db.Users là DbSet cho ApplicationUser
            if (user == null)
            {
                // Xử lý trường hợp không tìm thấy người dùng (ví dụ: ghi log lỗi)
                System.Diagnostics.Debug.WriteLine($"Warning: User with ID {customerId} not found when trying to update order names.");
                return;
            }

            string latestFullName = user.FullName;
            string logName = user.UserName;
            // Giả định ApplicationUser có thuộc tính FullName

            // 2. Tìm tất cả các đơn hàng của khách hàng này
            var ordersToUpdate = db.Orders.Where(o => o.CustomerId == customerId).ToList();

            // 3. Cập nhật CustomerName cho từng đơn hàng
            foreach (var orderItem in ordersToUpdate) // Đổi tên biến để tránh trùng với tham số order của hàm UpdateStatus
            {
                // Chỉ cập nhật nếu tên khác nhau để tránh đánh dấu thay đổi không cần thiết
                if (orderItem.CustomerName != latestFullName || orderItem.CreatedBy != logName)
                {
                    orderItem.CreatedBy = logName;
                    orderItem.CustomerName = latestFullName;
                }
            }
            // KHÔNG gọi db.SaveChanges() ở đây.
        }

    }
}