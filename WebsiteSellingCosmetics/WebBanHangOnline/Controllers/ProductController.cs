using Microsoft.Ajax.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Detail(int id)
        {
            var product = db.Products.Include("ReviewProducts")
                             .FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var averageRating = product.ReviewProducts.Any()
                ? product.ReviewProducts.Average(x => x.Rate)
                : 0;
            ViewBag.AverageRating = averageRating;
            var countReview = db.ReviewProducts.Where(x => x.ProductId == id).Count();
            ViewBag.CountReview = countReview;
            return View(product);
        }

        public ActionResult ProductCategory(string alias, float? priceMin, float? priceMax, int? id, int page = 1, string sortType = "original-order")
        {
            int pageSize = 8;
            IEnumerable<Product> items = db.Products.ToList();
            if (id != null)
            {
                items = items.Where(x => x.ProductCategory.Id == id);
                var cate = db.ProductCategories.Find(id);
                ViewBag.CateName = cate?.Title;
                ViewBag.Alias = alias;
            }
            else
            {
                ViewBag.Alias = "tat-ca";
            }
            ViewBag.CateId = id;

            if (priceMin.HasValue)
            {
                items = items.Where(x => x.Price >= priceMin.Value);
                ViewBag.PriceMin = priceMin;
            }
            if (priceMax.HasValue)
            {
                items = items.Where(x => x.Price <= priceMax.Value);
                ViewBag.PriceMax = priceMax;
            }

            IPagedList<Product> pagedItems;
            switch (sortType)
            {
                case "price":
                    pagedItems = items.OrderBy(x => x.Price).ToPagedList(page, pageSize); break;
                case "name":
                    pagedItems = items.OrderBy(x => x.Title).ToPagedList(page, pageSize); break;
                default:
                    pagedItems = items.OrderBy(x => x.CreatedDate).ToPagedList(page, pageSize); break;
            }
            ViewBag.SortType = sortType;
            return View(pagedItems);
        }

        public ActionResult ProductNew()
        {
            var items = db.Products
                .Where(x => x.IsActive)
                .GroupBy(x => x.ProductCategory.Title)
                .SelectMany(g => g.OrderByDescending(x => x.CreatedDate).Take(10))
                .OrderByDescending(x => x.CreatedDate)
                .ToList();
            return PartialView("_ProductNew", items);
        }

        public ActionResult ProductSales()
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var productSales = db.Products
                                .Join(
                                    db.OrderDetails.Join(
                                        db.Orders.Where(o => o.CreatedDate >= cutoffDate),
                                        od => od.OrderId,
                                        o => o.Id,
                                        (od, o) => od
                                    ),
                                    p => p.Id,
                                    od => od.ProductId,
                                    (p, od) => new { p, od.Quantity }
                                )
                                .GroupBy(x => x.p)
                                .Select(g => new
                                {
                                    Product = g.Key,
                                    TotalSold = g.Sum(x => x.Quantity)
                                })
                                .OrderByDescending(x => x.TotalSold)
                                .Take(8)
                                .ToList();
            ViewBag.ProductCounts = productSales.ToDictionary(x => x.Product.Id, x => x.TotalSold);
            var topProducts = productSales.Select(x => x.Product).ToList();

            var productList = new List<Product>();

            foreach (var topProduct in topProducts)
            {
                var images = db.ProductImages.Where(x => x.ProductId == topProduct.Id).ToList();
                var defaultImage = images.FirstOrDefault(x => x.IsDefault)?.Image ?? "/Uploads/images/No_Image_Available.jpg";
                var hoverImage = images.FirstOrDefault(x => x.IsHover)?.Image ?? "/Uploads/images/No_Image_Available.jpg";
                topProduct.ProductImage = new List<ProductImage>
                                          {
                                              new ProductImage { Image = defaultImage, IsDefault = true },
                                              new ProductImage { Image = hoverImage, IsHover = true }
                                          };
                productList.Add(topProduct);
            }
            return PartialView("_ProductSales", productList);
        }

        public ActionResult ProductRelated(int categoryId, int productId)
        {
            var items = db.Products
                          .Where(x => x.ProductCategoryId == categoryId
                                   && x.Id != productId
                                   && x.IsActive)
                          .OrderByDescending(x => x.CreatedDate)
                          .ToList();

            return PartialView("_ProductRelated", items);
        }

        public ActionResult ProductFeature()
        {
            var item = db.Products.Where(x => x.IsFeature).OrderByDescending(x => x.CreatedDate).ToList();
            return PartialView("_ProductFeature", item);
        }

        [HttpGet]
        public JsonResult SearchProducts(string searchTerm)
        {
            var normalizedSearchTerm = NormalizeVietnamese(searchTerm.ToLower());

            var products = db.Products
                .AsEnumerable()
                .Where(p => NormalizeVietnamese(p.Title.ToLower()).Contains(normalizedSearchTerm))
                .Select(p => new { p.Alias, p.Id, p.Title })
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        private string NormalizeVietnamese(string input)
        {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Replace special characters with their normalized form
            stringBuilder.Replace("đ", "d"); // Replace 'đ' with 'd'
            stringBuilder.Replace("Đ", "D"); // Replace 'Đ' with 'D'
                                             // Add more replacements if necessary

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}