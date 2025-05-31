using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace WebBanHangOnline.Models.EF
{
    [Table("Product")]
    public class Product : CommonAbstract
    {
        public Product()
        {
            this.ProductImage = new HashSet<ProductImage>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.ReviewProducts = new HashSet<ReviewProduct>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống!")]
        [StringLength(250)]
        public string Title { get; set; }

        [StringLength(250)]
        public string Alias { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một danh mục sản phẩm!")]
        public int ProductCategoryId { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; }

        public string Description { get; set; }

        [AllowHtml]
        public string Detail { get; set; }

        [StringLength(250)]
        public string Image { get; set; }

        [Required(ErrorMessage = "Giá gốc không được để trống!")]
        public int OriginalPrice { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống!")]
        public int Price { get; set; }

        public int? PriceSale { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống!")]
        public int Quantity { get; set; }

        public int SoldQuantity { get; set; }
        public bool IsHome { get; set; }
        public bool IsSale { get; set; }
        public bool IsFeature { get; set; }
        public bool IsHot { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn không được để trống!")]
        public DateTime ExpiredDate { get; set; }

        [StringLength(250)]
        public string SeoTitle { get; set; }

        [StringLength(500)]
        public string SeoDescription { get; set; }

        [StringLength(250)]
        public string SeoKeywords { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<ProductImage> ProductImage { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ReviewProduct> ReviewProducts { get; set; }
    }
}