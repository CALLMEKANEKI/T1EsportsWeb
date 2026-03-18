using Microsoft.AspNetCore.Mvc;
using T1EsportsWeb.Models;
using System.Linq;

namespace T1EsportsWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly T1DbContext _context;

        // Tiêm Database vào Controller
        public ShopController(T1DbContext context)
        {
            _context = context;
        }

        // Trang chủ của Shop (gọi khi vào /Shop) có nhận tham số sắp xếp
        [HttpGet]
        public IActionResult Index(string sortOrder)
        {
            // Lưu lại lựa chọn hiện tại để giữ trạng thái thẻ Select trên giao diện
            ViewBag.CurrentSort = sortOrder;

            // Lấy danh sách sản phẩm (AsQueryable để chưa chạy lệnh SQL vội)
            var products = _context.Products.AsQueryable();

            // Tiến hành sắp xếp dựa theo tín hiệu từ View gửi về
            switch (sortOrder)
            {
                case "price_asc":
                    // Giá: Thấp đến Cao
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    // Giá: Cao xuống Thấp
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                default:
                    // Mặc định: Sản phẩm mới nhất (dựa vào ID lớn nhất)
                    products = products.OrderByDescending(p => p.ProductId);
                    break;
            }

            return View(products.ToList());
        }

        // HÀM XEM CHI TIẾT SẢN PHẨM
        public IActionResult Details(int id)
        {
            // Tìm sản phẩm có ID khớp với ID trên đường dẫn
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return RedirectToAction("Index");
            }
            return View(product);
        }
    }
}