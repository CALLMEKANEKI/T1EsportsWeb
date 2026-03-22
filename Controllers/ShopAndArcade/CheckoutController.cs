using Microsoft.AspNetCore.Mvc;
using T1EsportsWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Collections.Generic;

namespace T1EsportsWeb.Controllers.ShopAndArcade
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly T1DbContext _context;

        public CheckoutController(T1DbContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ TRANG ĐIỀN THÔNG TIN
        [HttpGet]
        public IActionResult Index()
        {
            var cart = _context.CartItems
                               .Where(c => c.Username == User.Identity.Name)
                               .ToList();

            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            ViewBag.Cart = cart;

            var shippingSetting = _context.SystemSettings.FirstOrDefault(s => s.Key == "ShippingFee");
            decimal shippingFee = shippingSetting != null ? decimal.Parse(shippingSetting.Value) : 30000m;
            ViewBag.ShippingFee = shippingFee;

            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            return View(currentUser);
        }

        // 2. API KIỂM TRA MÃ GIẢM GIÁ (ĐÃ BỊT LỖ HỔNG XÀI LẠI MÃ)
        [HttpPost]
        public IActionResult ApplyVoucher(string voucherCode)
        {
            if (string.IsNullOrEmpty(voucherCode))
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá!" });

            voucherCode = voucherCode.Trim();

            // CHỐT CHẶN 1: Quét xem user này đã TỪNG DÙNG mã này mua hàng thành công chưa
            bool isAlreadyUsed = _context.UserVouchers.Any(v => v.VoucherCode == voucherCode && v.Username == User.Identity.Name && v.IsUsed);
            if (isAlreadyUsed)
            {
                return Json(new { success = false, message = "Rất tiếc! Mã giảm giá này đã được sử dụng cho đơn hàng trước đó rồi." });
            }

            // CHỐT CHẶN 2: Kiểm tra xem có phải mã cá nhân (chưa dùng) không
            var userVoucher = _context.UserVouchers.FirstOrDefault(v => v.VoucherCode == voucherCode && v.Username == User.Identity.Name && !v.IsUsed);
            if (userVoucher != null)
            {
                // KIỂM TRA HẠN SỬ DỤNG: Nếu ngày hiện tại đã vượt quá ngày hết hạn
                if (userVoucher.ExpirationDate < DateTime.Now)
                {
                    return Json(new { success = false, message = "Rất tiếc! Mã giảm giá này đã quá hạn sử dụng (30 ngày)." });
                }

                // Nếu còn hạn thì cho qua
                return Json(new { success = true, discount = userVoucher.DiscountPercent, message = $"<i class='fas fa-check-circle'></i> Áp dụng thành công mã giảm {userVoucher.DiscountPercent}%" });
            }

            // CHỐT CHẶN 3: Kiểm tra xem có phải mã sự kiện chung không
            var eventVoucher = _context.Vouchers.FirstOrDefault(v => v.EventCode == voucherCode);
            if (eventVoucher != null)
            {
                // Cho phép mã vô hạn (-1) đi qua, chỉ chặn mã có số lượng cụ thể mà đã cạn kiệt
                if (eventVoucher.Quantity <= 0 && eventVoucher.Quantity != -1)
                    return Json(new { success = false, message = "Rất tiếc! Mã sự kiện này đã hết số lượt sử dụng." });

                return Json(new { success = true, discount = eventVoucher.DiscountPercent, message = $"<i class='fas fa-check-circle'></i> Áp dụng thành công mã sự kiện giảm {eventVoucher.DiscountPercent}%" });
            }

            return Json(new { success = false, message = "Mã giảm giá không hợp lệ, đã hết hạn hoặc không thuộc về bạn." });
        }


        // 3. XỬ LÝ ĐẶT HÀNG NGẦM (THU HỒI VOUCHER TRIỆT ĐỂ)
        [HttpPost]
        public IActionResult ProcessOrder(string fullName, string phone, string address, string voucherCode)
        {
            var cart = _context.CartItems
                               .Where(c => c.Username == User.Identity.Name)
                               .ToList();

            if (cart == null || !cart.Any())
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });

            decimal originalTotal = cart.Sum(c => c.Total);
            int appliedDiscount = 0;

            var shippingSetting = _context.SystemSettings.FirstOrDefault(s => s.Key == "ShippingFee");
            decimal shippingFee = shippingSetting != null ? decimal.Parse(shippingSetting.Value) : 30000m;

            if (!string.IsNullOrEmpty(voucherCode))
            {
                voucherCode = voucherCode.Trim();

                var userVoucher = _context.UserVouchers.FirstOrDefault(v => v.VoucherCode == voucherCode && v.Username == User.Identity.Name && !v.IsUsed);
                if (userVoucher != null)
                {
                    appliedDiscount = userVoucher.DiscountPercent;
                    userVoucher.IsUsed = true; // Thu hồi mã cá nhân
                    _context.UserVouchers.Update(userVoucher);
                }
                else
                {
                    var eventVoucher = _context.Vouchers.FirstOrDefault(v => v.EventCode == voucherCode && (v.Quantity > 0 || v.Quantity == -1));
                    if (eventVoucher != null)
                    {
                        appliedDiscount = eventVoucher.DiscountPercent;

                        // Trừ số lượng kho chung (nếu không phải loại vô hạn)
                        if (eventVoucher.Quantity != -1)
                        {
                            eventVoucher.Quantity -= 1;
                            _context.Vouchers.Update(eventVoucher);
                        }

                        // Ép một bản ghi "Đã xài" vào kho của User để chặn dùng lại lần sau
                        var usedRecord = new UserVoucher
                        {
                            Username = User.Identity.Name,
                            VoucherCode = voucherCode,
                            DiscountPercent = eventVoucher.DiscountPercent,
                            IsUsed = true
                        };
                        _context.UserVouchers.Add(usedRecord);
                    }
                }
            }

            decimal discountAmount = originalTotal * appliedDiscount / 100;
            decimal finalTotal = originalTotal - discountAmount + shippingFee;

            var order = new Order
            {
                Username = User.Identity.Name,
                FullName = fullName,
                Phone = phone,
                Address = address,
                TotalAmount = finalTotal,
                OrderDate = DateTime.Now,
                Status = "Chờ xử lý"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Size = item.Size,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetails.Add(detail);

                // Trừ tồn kho sản phẩm
                var product = _context.Products.Find(item.ProductId);
                if (product != null) product.StockQuantity -= item.Quantity;
            }

            _context.SaveChanges();

            // Dọn sạch giỏ hàng
            _context.CartItems.RemoveRange(cart);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}