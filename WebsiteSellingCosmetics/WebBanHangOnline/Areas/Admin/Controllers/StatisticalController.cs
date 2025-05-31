using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate, string viewMode)
        {
            DateTime startDate;
            DateTime endDate;

            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                endDate = DateTime.Now.Date;
                startDate = endDate.AddDays(-14); // 15 ngày gần nhất
            }
            else
            {
                startDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var query = db.Orders
                        .Where(o => o.Status == 2) // chỉ lấy đơn đã giao
                        .Join(db.OrderDetails,
                              o => o.Id,
                              od => od.OrderId,
                              (o, od) => new { o, od })
                        .Join(db.Products,
                              combined => combined.od.ProductId,
                              p => p.Id,
                              (combined, p) => new { combined.o, combined.od, p })
                        .Where(x => DbFunctions.TruncateTime(x.o.CreatedDate) >= startDate &&
                                    DbFunctions.TruncateTime(x.o.CreatedDate) <= endDate)
                        .Select(x => new
                        {
                            x.o.CreatedDate,
                            x.od.Quantity,
                            x.od.Price,
                            x.p.OriginalPrice
                        });

            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate))
                        .Select(x => new
                        {
                            Date = x.Key.Value,
                            TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                            TotalSell = x.Sum(y => y.Quantity * y.Price),
                        }).Select(x => new
                        {
                            x.Date,
                            Revenue = x.TotalSell,
                            //Profit = x.TotalSell - x.TotalBuy,
                        }).OrderBy(x => x.Date).ToList(); // Sort by date ascending

            // Fill in missing dates with zero revenue and profit
            var dateRange = Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                                      .Select(offset => startDate.AddDays(offset))
                                      .ToList();

            var finalResult = dateRange
                             .GroupJoin(
                                 result,
                                 date => date,
                                 data => data.Date,
                                 (date, gj) => new { date, gj })
                             .SelectMany(
                                 x => x.gj.DefaultIfEmpty(),
                                 (x, subData) => new
                                 {
                                     Date = x.date,
                                     Revenue = subData?.Revenue ?? 0,
                                     // Profit = subData?.Profit ?? 0
                                 });


            // Group by viewMode
            var groupedResult = GroupByMode(finalResult, viewMode);

            return Json(new { Data = groupedResult }, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<dynamic> GroupByMode(IEnumerable<dynamic> data, string viewMode)
        {
            switch (viewMode)
            {
                case "month":
                    return data.GroupBy(d => new { d.Date.Year, d.Date.Month })
                               .Select(g => new
                               {
                                   Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                                   Revenue = g.Sum(d => d.Revenue),
                                   //Profit = g.Sum(d => d.Profit)
                               })
                               .OrderByDescending(d => d.Date); // Sort by date ascending

                case "year":
                    return data.GroupBy(d => d.Date.Year)
                               .Select(g => new
                               {
                                   Date = new DateTime(g.Key, 1, 1),
                                   Revenue = g.Sum(d => d.Revenue),
                                   //Profit = g.Sum(d => d.Profit)
                               })
                               .OrderByDescending(d => d.Date); // Sort by date ascending

                case "day":
                default:
                    return data.OrderByDescending(d => d.Date); ;
            }
        }

        public ActionResult GetProductStatistics(string sortField = "soldQuantity", string sortOrder = "desc")
        {
            var paidOrderDetails = db.OrderDetails.Where(od => od.Order.Status == 2); // chỉ lấy chi tiết từ đơn đã giao
            var query = db.Products
                        .GroupJoin(
                            paidOrderDetails,
                            p => p.Id,
                            od => od.ProductId,
                            (p, productSales) => new { p, productSales })
                        .SelectMany(
                            x => x.productSales.DefaultIfEmpty(),
                            (x, od) => new { x.p, od })
                        .GroupBy(
                            x => new
                            {
                                x.p.Id,
                                x.p.Title,
                                x.p.Quantity,
                                x.p.ExpiredDate,
                                ProductImage = x.p.ProductImage.FirstOrDefault(img => img.IsDefault) != null
                                    ? x.p.ProductImage.FirstOrDefault(img => img.IsDefault).Image
                                    : "/Uploads/images/No_Image_Available.jpg"
                            })
                        .Select(g => new
                        {
                            ProductId = g.Key.Id,
                            g.Key.ProductImage,
                            ProductName = g.Key.Title,
                            SoldQuantity = g.Sum(x => x.od == null ? 0 : x.od.Quantity),
                            RemainingQuantity = g.Key.Quantity,
                            ExpiredDate = g.Key.ExpiredDate
                        });


            // Sorting
            switch (sortField)
            {
                case "soldQuantity":
                    query = sortOrder == "desc" ? query.OrderByDescending(x => x.SoldQuantity) : query.OrderBy(x => x.SoldQuantity);
                    break;
                case "remainingQuantity":
                    query = sortOrder == "desc" ? query.OrderByDescending(x => x.RemainingQuantity) : query.OrderBy(x => x.RemainingQuantity);
                    break;
                case "productName":
                    query = sortOrder == "desc" ? query.OrderByDescending(x => x.ProductName) : query.OrderBy(x => x.ProductName);
                    break;
                case "expiredDate":
                    query = sortOrder == "desc" ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate);
                    break;
            }

            var result = query.ToList();
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTopProducts(bool isTop = true, int count = 5)
        {
            var paidOrderDetails = db.OrderDetails.Where(od => od.Order.Status == 2); // chỉ lấy chi tiết từ đơn đã giao
            var query = db.Products
                        .GroupJoin(
                            paidOrderDetails,
                            p => p.Id,
                            od => od.ProductId,
                            (p, productSales) => new { p, productSales })
                        .SelectMany(
                            x => x.productSales.DefaultIfEmpty(),
                            (x, od) => new { x.p, od })
                        .GroupBy(
                            x => new
                            {
                                x.p.Id,
                                x.p.Title,
                                ProductImage = x.p.ProductImage.FirstOrDefault(img => img.IsDefault) != null
                                    ? x.p.ProductImage.FirstOrDefault(img => img.IsDefault).Image
                                    : "/Uploads/images/No_Image_Available.jpg"
                            })
                        .Select(g => new
                        {
                            ProductId = g.Key.Id,
                            g.Key.ProductImage,
                            ProductName = g.Key.Title,
                            SoldQuantity = g.Sum(x => x.od == null ? 0 : x.od.Quantity)
                        });

            // Get either top or bottom products
            var result = isTop
                ? query.OrderByDescending(x => x.SoldQuantity).Take(count).ToList()
                : query.OrderBy(x => x.SoldQuantity).Take(count).ToList();

            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
