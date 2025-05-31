using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models
{
    public class ShoppingCart
    {
        public string UserId { get; set; }
        public List<ShoppingCartItem> Items { get; set; }

        public ShoppingCart(string userId = null)
        {
            this.Items = new List<ShoppingCartItem>();
            this.UserId = userId;
        }

        // Hàm tải giỏ hàng từ cơ sở dữ liệu hoặc session
        public static ShoppingCart LoadCart(string userId, ApplicationDbContext db)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var dbCart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == userId);
                if (dbCart != null)
                {
                    ShoppingCart cart = new ShoppingCart(userId);
                    cart.Items = dbCart.CartItems.Join(
                        db.Products,
                        cartItem => cartItem.ProductId,
                        product => product.Id,
                        (cartItem, product) => new ShoppingCartItem
                        {
                            ProductId = cartItem.ProductId,
                            ProductName = cartItem.ProductName,
                            Alias = cartItem.Alias,
                            ProductImg = cartItem.ProductImg,
                            CategoryName = cartItem.CategoryName,
                            Price = product.Price,
                            Quantity = cartItem.Quantity,
                            LeftQuantity = product.Quantity,
                            TotalPrice = cartItem.TotalPrice,
                            IsExpired = (product.ExpiredDate - DateTime.Now).TotalDays <= 10
                        }).ToList();
                    return cart;
                }
            }
            return (ShoppingCart)HttpContext.Current.Session["Cart"] ?? new ShoppingCart();
        }

        public void AddToCart(ShoppingCartItem item, ApplicationDbContext db)
        {
            var cart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == UserId);
            var cartItem = cart.CartItems.SingleOrDefault(x => x.ProductId == item.ProductId);            
            if (cartItem != null)
            {
                cartItem.Quantity += item.Quantity;
                cartItem.Quantity = Math.Min(item.LeftQuantity, cartItem.Quantity);
                cartItem.TotalPrice = cartItem.Price * cartItem.Quantity;
                db.SaveChanges();

                var _cartItem = Items.SingleOrDefault(x => x.ProductId == item.ProductId);
                _cartItem.Quantity = cartItem.Quantity;
                _cartItem.TotalPrice = cartItem.TotalPrice;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Alias = item.Alias,
                    ProductImg = item.ProductImg,
                    CategoryName = item.CategoryName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                });
                db.SaveChanges();

                Items.Add(item);
            }
        }

        public void Remove(int id, ApplicationDbContext db)
        {
            var cart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == UserId);
            var cartItem = cart.CartItems.SingleOrDefault(x => x.ProductId == id && x.CartId == cart.Id);
            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();

                Items.Remove(Items.SingleOrDefault(x => x.ProductId == id));
            }
        }

        public void UpdateItemCartQuantity(int id, int quantity, ApplicationDbContext db)
        {
            var cart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == UserId);
            var cartItem = cart.CartItems.SingleOrDefault(x => x.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                cartItem.TotalPrice = cartItem.Price * cartItem.Quantity;
                db.SaveChanges();

                var _cartItem = Items.SingleOrDefault(x => x.ProductId == id);
                _cartItem.Quantity = quantity;
                _cartItem.TotalPrice = cartItem.TotalPrice;
            }
        }

        public void UpdateProductQuantity(Order order, ApplicationDbContext db)
        {
            foreach (var detail in order.OrderDetails)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == detail.ProductId);

                if (product != null)
                {
                    product.Quantity = Math.Max(product.Quantity - detail.Quantity, 0);
                    product.SoldQuantity += detail.Quantity;
                    db.Entry(product).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
        }

        public int GetTotalPrice()
        {
            return Items.Sum(x => x.TotalPrice);
        }

        public int GetTotalQuantity()
        {
            return Items.Sum(x => x.Quantity);
        }

        public void ClearItemCart(List<int> selectedProductIds, ApplicationDbContext db) 
        {
            Items.RemoveAll(x => selectedProductIds.Contains((int)x.GetType().GetProperty("ProductId").GetValue(x, null)));

            var cart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == UserId);
            foreach (var id in selectedProductIds)
            {
                db.CartItems.Remove(cart.CartItems.SingleOrDefault(x => x.ProductId == id && x.CartId == cart.Id));
            }
            db.SaveChanges();
        }

        public void ClearAllCart(ApplicationDbContext db)
        {
            Items.Clear();
            var cart = db.Carts.Include("CartItems").SingleOrDefault(c => c.UserId == UserId);
            foreach (var cartItem in cart.CartItems.ToList())
            {
                db.CartItems.Remove(cartItem);
            }

            db.SaveChanges();
        }
    }

    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public string CategoryName { get; set; }
        public string ProductImg { get; set; }
        public int Quantity { get; set; }
        public int LeftQuantity { get; set; }
        public int Price { get; set; }
        public int TotalPrice { get; set; }
        public bool IsExpired { get; set; }
    }
}
