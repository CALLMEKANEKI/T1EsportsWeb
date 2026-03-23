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

namespace T1EsportsWeb.Areas.Admin.Controllers.ShopExtensions
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
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
            try
            {
                // 1. TỰ ĐỘNG SINH MÃ NẾU BOSS KHÔNG NHẬP (Định dạng: T1-XXXXXX)
                if (string.IsNullOrWhiteSpace(voucher.EventCode))
                {
                    var random = new Random();
                    const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    const string numbers = "0123456789";
                    const string allChars = letters + numbers;
                    var codeChars = new char[6];
                    codeChars[0] = letters[random.Next(letters.Length)];
                    codeChars[1] = numbers[random.Next(numbers.Length)];
                    for (int i = 2; i < 6; i++)
                    {
                        codeChars[i] = allChars[random.Next(allChars.Length)];
                    }
                    codeChars = codeChars.OrderBy(x => random.Next()).ToArray();
                    voucher.EventCode = "T1-" + new string(codeChars);
                }
                else
                {
                    voucher.EventCode = voucher.EventCode.Trim().ToUpper();
                }
                if (string.IsNullOrWhiteSpace(voucher.Title))
                {
                    voucher.Title = "Thẻ Quà Tặng T1 Shop";
                }
                //Lưu vào Database
                _context.Vouchers.Add(voucher);
                _context.SaveChanges();
                // Gửi thông báo xịn xò về lại View
                TempData["SuccessMsg"] = $"Tạo thẻ quà thành công! Mã: {voucher.EventCode}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Lỗi khi tạo thẻ quà: " + ex.Message;
            }

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
        // 5. CHĂM SÓC KHÁCH HÀNG (CHAT) - ĐÃ FIX LỖI GỬI/NHẬN
        // ==========================================

        // 1. Danh sách khách hàng (Chỉ Admin/Staff xem được)
        public IActionResult CustomerSupport()
        {
            var chatList = _context.ChatMessages
                .Where(m => m.SenderUsername != "Admin" && m.SenderUsername != "adminweb")
                .OrderByDescending(m => m.Timestamp)
                .ToList()
                .GroupBy(m => m.SenderUsername)
                .Select(g => g.First())
                .ToList();
            return View(chatList);
        }

        // 2. Phòng chat Admin
        public IActionResult ChatRoom(string user)
        {
            ViewBag.TargetUser = user;
            return View();
        }

        // ==========================================
        // QUẢN LÝ DỰ ĐOÁN PICK'EM
        // ==========================================
        public IActionResult ManagePickEm()
        {
            var matches = _context.PickEmMatches.OrderByDescending(m => m.MatchTime).ToList();
            return View(matches);
        }

        [HttpPost]
        public IActionResult RewardPickEmMatch(int matchId, string realScore)
        {
            var match = _context.PickEmMatches.Find(matchId);
            if (match == null) return Json(new { success = false, message = "Không tìm thấy trận đấu!" });
            if (match.IsRewarded) return Json(new { success = false, message = "Đã phát thưởng rồi!" });

            match.ActualScore = realScore;
            match.IsLocked = true;
            match.IsRewarded = true;

            var winningPicks = _context.PickEmPredictions
                .Where(p => p.SeriesId == matchId && p.PredictedScore == realScore && !p.IsProcessed).ToList();

            int countWinners = 0;
            foreach (var pick in winningPicks)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == pick.Username);
                if (user != null)
                {
                    user.T1Points += 100; // Cộng 100 Points
                    pick.IsProcessed = true;
                    countWinners++;
                }
            }

            _context.SaveChanges();
            return Json(new { success = true, message = $"Đã chốt tỉ số {realScore} và phát 100 Points cho {countWinners} người đoán trúng!" });
        }

        [HttpPost]
        public IActionResult AddPickEmMatch(string tournamentName, DateTime matchTime, string opponentName)
        {
            if (string.IsNullOrEmpty(tournamentName) || string.IsNullOrEmpty(opponentName))
            {
                return Json(new { success = false, message = "Vui lòng nhập đủ thông tin!" });
            }

            var newMatch = new PickEmMatch
            {
                TournamentName = tournamentName,
                MatchTime = matchTime,
                OpponentName = opponentName,
                IsLocked = false,
                IsRewarded = false,
                ActualScore = ""
            };

            _context.PickEmMatches.Add(newMatch);
            _context.SaveChanges();
            return Json(new { success = true, message = "Thêm trận đấu thành công!" });
        }
        [HttpPost]
        public IActionResult DeletePickEmMatch(int matchId)
        {
            var match = _context.PickEmMatches.Find(matchId);
            if (match == null) return Json(new { success = false, message = "Không tìm thấy trận đấu!" });

            // Tìm và xóa luôn các dự đoán của người chơi liên quan đến trận này (tránh lỗi khóa ngoại)
            var relatedPicks = _context.PickEmPredictions.Where(p => p.SeriesId == matchId).ToList();
            if (relatedPicks.Any())
            {
                _context.PickEmPredictions.RemoveRange(relatedPicks);
            }

            // Xóa trận đấu
            _context.PickEmMatches.Remove(match);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa trận đấu thành công!" });
        }

        // ==========================================
        // 6. QUẢN LÝ NHÂN SỰ VÀ PHÂN QUYỀN (3 CẤP)
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult UserList()
        {
            // Chặn ngay lập tức nếu Staff (Nhân viên) cố tình gõ link /Admin/UserList để vào trộm
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }

            // Sắp xếp theo thứ bậc: Admin -> Staff -> User
            var users = _context.Users
                .OrderBy(u => u.Role == "Admin" ? 1 : u.Role == "Staff" ? 2 : 3)
                .ThenBy(u => u.UserId)
                .ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult ChangeRole(int userId, string newRole)
        {
            // Chỉ Admin mới có quyền thực hiện đổi chức vụ
            if (!User.IsInRole("Admin")) return RedirectToAction("Index", "Home");

            var currentUser = _context.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser != null && currentUser.UserId == userId)
            {
                TempData["Error"] = "Boss ơi, Boss không thể tự giáng chức chính mình được đâu!";
                return RedirectToAction("UserList");
            }

            var targetUser = _context.Users.Find(userId);
            if (targetUser != null && (newRole == "Admin" || newRole == "Staff" || newRole == "User"))
            {
                targetUser.Role = newRole;
                _context.SaveChanges();
                TempData["Success"] = $"Đã cập nhật chức vụ của {targetUser.Username} thành {newRole}!";
            }
            return RedirectToAction("UserList");
        }

    }
}