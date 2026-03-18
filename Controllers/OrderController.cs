using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using T1EsportsWeb.Models;
using System.Linq;

namespace T1EsportsWeb.Controllers
{
    [Authorize] // Phải đăng nhập mới được xem đơn hàng của mình
    public class OrderController : Controller
    {
        private readonly T1DbContext _context;

        public OrderController(T1DbContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH ĐƠN HÀNG CỦA TÔI
        public IActionResult Index()
        {
            // Chỉ lấy những đơn hàng có Username trùng với người đang đăng nhập
            var username = User.Identity.Name;
            var myOrders = _context.Orders
                .Where(o => o.Username == username)
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất xếp lên đầu
                .ToList();

            return View(myOrders);
        }

        // 2. TRANG CHI TIẾT 1 ĐƠN HÀNG (Xem trong đơn đó mua áo gì, size gì)
        public IActionResult Details(int id)
        {
            var username = User.Identity.Name;

            // Tìm đúng hóa đơn đó, và phải đảm bảo nó là của người này
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id && o.Username == username);
            if (order == null) return NotFound();

            // Tìm danh sách các món đồ nằm trong hóa đơn này
            var orderDetails = _context.OrderDetails.Where(od => od.OrderId == id).ToList();

            // Ném qua View
            ViewBag.OrderDetails = orderDetails;

            return View(order);
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            // Tìm đơn hàng đúng của user đang đăng nhập
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId && o.Username == User.Identity.Name);

            if (order != null && order.Status == "Chờ xử lý")
            {
                order.Status = "Đã hủy"; // Cập nhật trạng thái

                // Hoàn lại số lượng sản phẩm vào kho hệ thống
                var details = _context.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                foreach (var item in details)
                {
                    var product = _context.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity; // Cộng trả lại
                    }
                }

                _context.SaveChanges();
                TempData["SuccessMsg"] = "Đã hủy đơn hàng thành công!";
            }
            else
            {
                TempData["ErrorMsg"] = "Không thể hủy đơn hàng này do Admin đã xác nhận hoặc đang giao!";
            }

            return RedirectToAction("Index"); // Quay lại trang lịch sử đơn
        }
    }
}