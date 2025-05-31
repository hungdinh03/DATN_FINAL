using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Product
        public ActionResult Index(string searchText, int page = 1)
        {
            var pageSize = 10;
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id).ToList();
            foreach (var item in items)
            {
                var defaultImage = item.ProductImage.FirstOrDefault(x => x.IsDefault);
                // Nếu sản phẩm không có ảnh lúc thêm
                if (defaultImage == null)
                {
                    item.ProductImage.Add(new ProductImage
                    {
                        Image = "/Uploads/images/No_Image_Available.jpg",  // Thay thế bằng đường dẫn ảnh mặc định
                        IsDefault = true,
                    });
                }
            }
            
            if (!string.IsNullOrEmpty(searchText))
            {
                items = items.Where(x => x.Title.ToLower().Contains(searchText.ToLower()) || x.ProductCategory.Title.ToLower().Contains(searchText.ToLower()));
            }
            items = items.ToPagedList(page, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            ViewBag.SearchText = searchText;
            return View(items);
        }

        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault, List<int> rHover, List<int> rFeature)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        var isDefault = rDefault != null && rDefault.Contains(i + 1);
                        var isHover = rHover != null && rHover.Contains(i + 1);
                        var isFeature = rFeature != null && rFeature.Contains(i + 1);

                        model.ProductImage.Add(new ProductImage
                        {
                            ProductId = model.Id,
                            Image = Images[i],
                            IsDefault = isDefault,
                            IsHover = isHover,
                            IsFeature = isFeature,
                        });

                        // Nếu là ảnh mặc định thì lưu vào trường Image chính của sản phẩm
                        if (isDefault)
                        {
                            model.Image = Images[i];
                        }
                    }
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = User.Identity.Name;
                model.ModifiedBy = User.Identity.Name;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Add(model);
                db.SaveChanges();
                return Json(new { success = true });            
            }
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return Json(new { success = false });
        }

        public ActionResult Edit(int id)
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            var item = db.Products.Find(id);

            // Lấy danh sách ảnh của sản phẩm
            ViewBag.ProductImages = db.ProductImages.Where(x => x.ProductId == id).ToList();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model, List<string> Images, List<int> rDefault, List<int> rHover, List<int> rFeature)
        {
            if (ModelState.IsValid)
            {
                var item = db.Products.Find(model.Id);
                if (item != null)
                {
                    // Cập nhật thông tin sản phẩm
                    item.Title = model.Title;
                    item.ProductCode = model.ProductCode;
                    item.ProductCategoryId = model.ProductCategoryId;
                    item.Description = model.Description;
                    item.Detail = model.Detail;
                    item.Quantity = model.Quantity;
                    item.OriginalPrice = model.OriginalPrice;
                    item.Price = model.Price;
                    item.PriceSale = model.PriceSale;
                    item.ExpiredDate = model.ExpiredDate;
                    item.SeoTitle = model.SeoTitle;
                    item.SeoDescription = model.SeoDescription;
                    item.SeoKeywords = model.SeoKeywords;
                    item.IsHome = model.IsHome;
                    item.IsActive = model.IsActive;
                    item.IsHot = model.IsHot;
                    item.IsFeature = model.IsFeature;
                    item.IsSale = model.IsSale;
                    item.ModifiedDate = DateTime.Now;
                    item.ModifiedBy = User.Identity.Name;

                    // Xóa hình ảnh cũ nếu có
                    if (item.ProductImage != null && item.ProductImage.Any())
                    {
                        db.ProductImages.RemoveRange(item.ProductImage);
                    }

                    // Thêm hình ảnh mới
                    if (Images != null && Images.Any())
                    {
                        for (int i = 0; i < Images.Count; i++)
                        {
                            var isDefault = rDefault != null && rDefault.Contains(i + 1);
                            var isHover = rHover != null && rHover.Contains(i + 1);
                            var isFeature = rFeature != null && rFeature.Contains(i + 1);
                            var img = new ProductImage
                            {
                                ProductId = item.Id,
                                Image = Images[i],
                                IsDefault = isDefault,
                                IsHover = isHover,
                                IsFeature = isFeature,
                            };

                            item.ProductImage.Add(img);

                            // Cập nhật ảnh đại diện
                            if (isDefault)
                            {
                                item.Image = img.Image;
                            }
                        }
                    }
                    else
                    {
                        // Nếu không có ảnh mới, kiểm tra ảnh đại diện cũ
                        var defaultImage = item.ProductImage.FirstOrDefault(x => x.IsDefault);
                        if (defaultImage != null)
                        {
                            item.Image = defaultImage.Image;
                        }
                    }

                    if (string.IsNullOrEmpty(item.SeoTitle))
                    {
                        item.SeoTitle = item.Title;
                    }
                    item.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(item.Title);

                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }

            // Nếu có lỗi hoặc không hợp lệ, hiển thị lại view
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                db.Products.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        
        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isActive = item.IsActive });
            }
            return Json(new { isActive = false });
        }

        [HttpPost]
        public ActionResult IsFeature(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsFeature = !item.IsFeature;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isFeature = item.IsFeature });
            }
            return Json(new { isFeature = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Products.Find(Convert.ToInt32(item));
                        db.Products.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}