using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using T1EsportsWeb.Models;
using System.Linq;
using System;

namespace T1EsportsWeb.Controllers
{
    public class ArcadeController : Controller
    {
        private readonly T1DbContext _context;

        public ArcadeController(T1DbContext context)
        {
            _context = context;
        }

        // Sảnh chờ Arcade
        public IActionResult Index()
        {
            return View();
        }

        // 1. Game T1 Trivia
        public IActionResult Quizgame()
        {
            return View();
        }

        // 2. Game Flappy ATI
        public IActionResult FlappyATI()
        {
            return View();
        }

        // 3. Game Oracle Pick'em
        public IActionResult PickEm()
        {
            return View();
        }

        // ==========================================
        // KHU VỰC XỬ LÝ TRUNG TÂM ĐỔI THƯỞNG
        // ==========================================

        [Authorize]
        public IActionResult RewardCenter()
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            ViewBag.CurrentPoints = currentUser != null ? currentUser.T1Points : 0;

            ViewBag.ShopVouchers = _context.Vouchers
                .Where(v => v.CostPoints >= 0 && (v.Quantity > 0 || v.Quantity == -1))
                .OrderBy(v => v.CostPoints)
                .ToList();

            ViewBag.MyVouchers = _context.UserVouchers
                .Where(v => v.Username == User.Identity.Name && !v.IsUsed)
                .OrderByDescending(v => v.Id)
                .ToList();

            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Redeem(int voucherId)
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            var targetVoucher = _context.Vouchers.Find(voucherId);

            // 1. Kiểm tra kho hàng
            if (targetVoucher == null || (targetVoucher.Quantity <= 0 && targetVoucher.Quantity != -1))
            {
                TempData["ErrorMsg"] = "Rất tiếc! Gói Voucher này vừa bị người khác đổi hết lượt rồi.";
                return RedirectToAction("RewardCenter");
            }

            // 2. Kiểm tra xem người dùng đã đổi chưa (CHỈ ÁP DỤNG KHI ADMIN CÓ GHI MÃ CỐ ĐỊNH)
            if (targetVoucher.Quantity != -1 && !string.IsNullOrEmpty(targetVoucher.EventCode))
            {
                bool hasRedeemed = _context.UserVouchers.Any(v =>
                    v.Username == User.Identity.Name &&
                    v.VoucherCode == targetVoucher.EventCode); // So sánh khớp 100% mã gốc

                if (hasRedeemed)
                {
                    TempData["ErrorMsg"] = "Bạn đã đổi mã này rồi, hãy nhường cơ hội cho các fan khác nhé!";
                    return RedirectToAction("RewardCenter");
                }
            }

            // 3. Kiểm tra số dư điểm
            if (currentUser == null || currentUser.T1Points < targetVoucher.CostPoints)
            {
                TempData["ErrorMsg"] = "Úi, bạn chưa cày đủ T1 Points để đổi chiếc Voucher này rồi!";
                return RedirectToAction("RewardCenter");
            }

            // 4. Trừ điểm
            currentUser.T1Points -= targetVoucher.CostPoints;

            // 5. Trừ số lượng kho
            if (targetVoucher.Quantity != -1)
            {
                targetVoucher.Quantity -= 1;
            }

            // 6. XỬ LÝ LOGIC SINH MÀ (NGẪU NHIÊN HAY CỐ ĐỊNH)
            string finalVoucherCode = "";
            if (!string.IsNullOrEmpty(targetVoucher.EventCode))
            {
                // Admin đã điền mã -> Lấy y nguyên không random thêm
                finalVoucherCode = targetVoucher.EventCode;
            }
            else
            {
                // Admin để trống -> Random 6 ký tự
                string randomSuffix = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                finalVoucherCode = $"T1-{randomSuffix}";
            }

            var newVoucher = new UserVoucher
            {
                Username = currentUser.Username,
                DiscountPercent = targetVoucher.DiscountPercent,
                VoucherCode = finalVoucherCode,
                IsUsed = false,
                ExpirationDate = DateTime.Now.AddDays(30)
            };

            _context.UserVouchers.Add(newVoucher);
            _context.SaveChanges();

            TempData["SuccessMsg"] = $"Đổi thành công! Mã giảm {targetVoucher.DiscountPercent}%:{finalVoucherCode}";
            return RedirectToAction("RewardCenter");
        }

        [HttpPost]
        [Authorize]
        public IActionResult SavePoints(int points)
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser != null && points > 0)
            {
                currentUser.T1Points += points;
                _context.SaveChanges();
                return Json(new { success = true, totalPoints = currentUser.T1Points });
            }
            return Json(new { success = false });
        }
    }
}