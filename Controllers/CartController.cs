using Microsoft.AspNetCore.Mvc;
using T1EsportsWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace T1EsportsWeb.Controllers
{
    [Authorize] // Ép đăng nhập mới được dùng giỏ hàng
    public class CartController : Controller
    {
        private readonly T1DbContext _context;

        public CartController(T1DbContext context)
        {
            _context = context;
        }

        // 1. TRANG XEM GIỎ HÀNG (Lấy từ DB)
        public IActionResult Index()
        {
            var cart = _context.CartItems
                .Where(c => c.Username == User.Identity.Name)
                .ToList();
            return View(cart);
        }

        // 2. THÊM SẢN PHẨM (Lưu vào DB)
        [HttpPost]
        public IActionResult Add(int productId, int quantity, string selectedSize)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            // Tìm xem món này (cùng size) đã có trong giỏ của user này chưa
            var existingItem = _context.CartItems.FirstOrDefault(c =>
                c.ProductId == productId &&
                c.Size == selectedSize &&
                c.Username == User.Identity.Name);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    Username = User.Identity.Name,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImageUrl = string.IsNullOrEmpty(product.ImageUrl) ? "/images/default-product.png" : product.ImageUrl.Split(';')[0],
                    Price = product.Price,
                    Quantity = quantity,
                    Size = selectedSize
                });
            }

            _context.SaveChanges();

            int totalCount = _context.CartItems
                .Where(c => c.Username == User.Identity.Name)
                .Sum(c => c.Quantity);

            return Json(new { success = true, cartCount = totalCount });
        }

        // 3. XÓA MÓN HÀNG
        public IActionResult Remove(int productId, string size)
        {
            var itemToRemove = _context.CartItems.FirstOrDefault(c =>
                c.ProductId == productId &&
                c.Size == size &&
                c.Username == User.Identity.Name);

            if (itemToRemove != null)
            {
                _context.CartItems.Remove(itemToRemove);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // API dùng để gọi ngầm lấy số lượng giỏ hàng hiển thị lên Badge
        [HttpGet]
        public IActionResult GetCartBadgeCount()
        {
            if (User.Identity.IsAuthenticated)
            {
                int totalCartCount = _context.CartItems
                                             .Where(c => c.Username == User.Identity.Name)
                                             .Sum(c => (int?)c.Quantity) ?? 0;
                return Json(new { count = totalCartCount });
            }
            return Json(new { count = 0 });
        }
    }
}