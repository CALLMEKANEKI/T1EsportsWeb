using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using T1EsportsWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace T1EsportsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly T1DbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(T1DbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 0. DASHBOARD
        public IActionResult Index()
        {
            // 1. THỐNG KÊ NHANH (Top Cards)
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.PendingOrdersCount = _context.Orders.Count(o => o.Status == "Chờ xử lý");
            ViewBag.LowStockCount = _context.Products.Count(p => p.StockQuantity < 5);

            // 2. DANH SÁCH CẦN XỬ LÝ (Tăng lên 8 đơn hàng)
            ViewBag.RecentOrders = _context.Orders
                                           .OrderByDescending(o => o.OrderDate)
                                           .Take(8)
                                           .ToList();

            // 3. HOẠT ĐỘNG GẦN ĐÂY 
            // Tăng lên 5 lượt đổi Voucher
            ViewBag.RecentVouchers = _context.UserVouchers
                                             .OrderByDescending(uv => uv.Id)
                                             .Take(5)
                                             .ToList();

            // Tăng lên 5 tin nhắn
            ViewBag.RecentChats = _context.ChatMessages
                                          .Where(m => m.SenderUsername != "Admin" && m.SenderUsername != "adminweb")
                                          .OrderByDescending(m => m.Timestamp)
                                          .GroupBy(m => m.SenderUsername)
                                          .Select(g => g.First())
                                          .Take(5)
                                          .ToList();

            // 4. MỚI: SẢN PHẨM VỪA THÊM (Lấp đầy khoảng trống bên dưới)
            ViewBag.RecentProducts = _context.Products
                                             .OrderByDescending(p => p.ProductId)
                                             .Take(4)
                                             .ToList();

            return View();
        }

        // ==========================================
        // 1. QUẢN LÝ SẢN PHẨM (Code cũ của bạn giữ nguyên)
        // ==========================================
        public IActionResult AddProduct() => View();

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, List<IFormFile> imageFiles)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Sizes");
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    List<string> uploadedLinks = new List<string>();
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in imageFiles)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        uploadedLinks.Add("/images/products/" + uniqueFileName);
                    }
                    product.ImageUrl = string.Join(";", uploadedLinks);
                }
                else
                {
                    product.ImageUrl = "/images/default-product.png";
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = $"Đã thêm thành công sản phẩm: {product.ProductName}!";
                return RedirectToAction("ProductList");
            }
            return View(product);
        }

        public IActionResult ProductList()
        {
            var products = _context.Products.OrderByDescending(p => p.ProductId).ToList();
            return View(products);
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = $"Đã xóa sản phẩm {product.ProductName} khỏi kho!";
            }
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return RedirectToAction("ProductList");

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                ViewBag.OldImages = product.ImageUrl.Split(';').ToList();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Product product, List<IFormFile> imageFiles)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Sizes");
            ModelState.Remove("Description");

            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null) return NotFound();

                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Sizes = product.Sizes;
                existingProduct.Description = product.Description;

                List<string> finalImages = new List<string>();
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    finalImages.AddRange(product.ImageUrl.Split(';', StringSplitOptions.RemoveEmptyEntries));
                }

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    foreach (var file in imageFiles)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        finalImages.Add("/images/products/" + uniqueFileName);
                    }
                }

                existingProduct.ImageUrl = string.Join(";", finalImages);

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = $"Đã cập nhật thành công {product.ProductName}!";
                return RedirectToAction("ProductList");
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Json(new { success = false, message = "Không có sản phẩm nào được chọn!" });
            }

            var productsToDelete = _context.Products.Where(p => ids.Contains(p.ProductId)).ToList();

            if (productsToDelete.Any())
            {
                _context.Products.RemoveRange(productsToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = $"Đã xóa thành công {productsToDelete.Count} sản phẩm khỏi kho!";
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Lỗi: Không tìm thấy sản phẩm trong Database." });
        }


        // ==========================================
        // 2. QUẢN LÝ ĐƠN HÀNG (TÍNH NĂNG MỚI)
        // ==========================================
        public IActionResult OrderList()
        {
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null && !string.IsNullOrEmpty(newStatus))
            {
                order.Status = newStatus;
                _context.Orders.Update(order);
                _context.SaveChanges();
                TempData["SuccessMsg"] = $"Đã cập nhật trạng thái đơn #T1-{orderId} thành '{newStatus}'.";
            }
            return RedirectToAction("OrderList");
        }

        public IActionResult OrderDetail(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return RedirectToAction("OrderList");

            var details = _context.OrderDetails.Where(od => od.OrderId == id).ToList();
            ViewBag.Details = details;
            return View(order);
        }

        // ==========================================
        // 3. QUẢN LÝ VOUCHER SHOP (TÍNH NĂNG MỚI)
        // ==========================================
        public IActionResult VoucherList()
        {
            var vouchers = _context.Vouchers.OrderByDescending(v => v.Id).ToList();
            return View(vouchers);
        }

        [HttpPost]
        public IActionResult AddVoucher(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            _context.SaveChanges();
            TempData["SuccessMsg"] = "Đã thêm Voucher mới thành công!";
            return RedirectToAction("VoucherList");
        }

        public IActionResult DeleteVoucher(int id)
        {
            var v = _context.Vouchers.Find(id);
            if (v != null)
            {
                _context.Vouchers.Remove(v);
                _context.SaveChanges();
                TempData["SuccessMsg"] = "Đã xóa Voucher thành công!";
            }
            return RedirectToAction("VoucherList");
        }

        // ==========================================
        // 4. LỊCH SỬ ĐỔI ĐIỂM CỦA USER (TÍNH NĂNG MỚI)
        // ==========================================
        public IActionResult RedemptionHistory()
        {
            // Lấy danh sách user đổi voucher
            var history = _context.UserVouchers.OrderByDescending(uv => uv.Id).ToList();
            return View(history);
        }

        // ==========================================
        // 5. CHĂM SÓC KHÁCH HÀNG (CHAT)
        // ==========================================

        // 1. Giao diện danh sách khách hàng cần hỗ trợ
        public IActionResult CustomerSupport()
        {
            // Lọc bỏ "Admin" và "adminweb" để không bị lọt vào danh sách khách hàng
            var chatList = _context.ChatMessages
                                   .Where(m => m.SenderUsername != "Admin" && m.SenderUsername != "adminweb")
                                   .OrderByDescending(m => m.Timestamp)
                                   .ToList()
                                   .GroupBy(m => m.SenderUsername)
                                   .Select(g => g.First())
                                   .ToList();

            return View(chatList);
        }

        // 2. Giao diện Phòng chat của Admin với 1 khách (Hàm này lúc nãy bị thiếu nè)
        public IActionResult ChatRoom(string user)
        {
            ViewBag.TargetUser = user;
            return View();
        }

        // 3. API để Gửi tin nhắn
        [AllowAnonymous]
        [HttpPost]
        public IActionResult SendMessage(string messageContent, string receiver)
        {
            // Logic chuẩn: Nếu gửi tới "Admin" -> Người gửi là Khách. Ngược lại là "Admin".
            string senderName = (receiver == "Admin")
                ? (User.Identity.IsAuthenticated ? User.Identity.Name : "Khách ẩn danh")
                : "Admin";

            var msg = new ChatMessage
            {
                SenderUsername = senderName,
                ReceiverUsername = receiver,
                MessageContent = messageContent,
                Timestamp = DateTime.Now
            };
            _context.ChatMessages.Add(msg);
            _context.SaveChanges();
            return Ok();
        }

        // 4. API để Lấy tin nhắn
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetMessages(string withUser)
        {
            // Logic chuẩn: Lấy tin nhắn với "Admin" -> Người đang lấy là Khách. Ngược lại là "Admin".
            string currentUser = (withUser == "Admin")
                ? (User.Identity.IsAuthenticated ? User.Identity.Name : "Khách ẩn danh")
                : "Admin";

            var msgs = _context.ChatMessages
                .Where(m => (m.SenderUsername == currentUser && m.ReceiverUsername == withUser) ||
                            (m.SenderUsername == withUser && m.ReceiverUsername == currentUser))
                .OrderBy(m => m.Timestamp)
                .ToList();

            return Json(msgs);
        }
    }
}