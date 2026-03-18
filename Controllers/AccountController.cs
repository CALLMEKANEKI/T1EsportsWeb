using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using T1EsportsWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Google;

namespace T1EsportsWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly T1DbContext _context;

        public AccountController(T1DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Đã sửa thành PasswordHash cho khớp với Database của bạn
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, string.IsNullOrEmpty(user.Role) ? "User" : user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã được sử dụng";
                return View();
            }

            // ĐÃ CHỈNH SỬA KHỚP 100% VỚI BẢNG USER.CS (Bao gồm cả Email)
            var newUser = new T1EsportsWeb.Models.User
            {
                Username = username,
                Email = email,           // Đã thêm lưu Email
                PasswordHash = password, // Dùng PasswordHash
                FullName = username,     // Tạm lấy username làm FullName vì form đăng ký đang không có ô FullName
                Role = "User",
                T1Points = 0             // Điểm cày game khởi đầu là 0
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký thành công rồi. Đăng nhập ngay";
            return RedirectToAction("Login");
        }

        // 1. Hàm gọi màn hình đăng nhập của Google
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // 2. Hàm hứng dữ liệu sau khi Google xác nhận thành công
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return RedirectToAction("Login");

            // Lấy thông tin Email và Tên từ Google trả về
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (email != null)
            {
                // CÁCH SỬA LỖI: Tách chuỗi email ở bên ngoài câu truy vấn LINQ
                string emailPrefix = email.Split('@')[0];

                // Truy vấn Database bằng biến đã tách
                var user = _context.Users.FirstOrDefault(u => u.Username == email || u.Username == emailPrefix);

                if (user == null)
                {
                    // NẾU CHƯA CÓ: Tự động tạo tài khoản mới cho khách
                    user = new User
                    {
                        Username = emailPrefix, // Lấy biến đã tách ở trên
                        Email = email,          // Đã thêm lưu Email từ Google
                        PasswordHash = "GoogleLoginUser",
                        FullName = name ?? emailPrefix,
                        Role = "User",
                        T1Points = 0
                    };
                    _context.Users.Add(user);
                    _context.SaveChanges();
                }

                // TIẾN HÀNH ĐĂNG NHẬP VÀ LƯU COOKIE
                var userClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Không thể lấy thông tin từ Google.";
            return View("Login");
        }

        // Hiển thị giao diện Quên Mật Khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Xử lý logic Reset Mật Khẩu
        [HttpPost]
        public IActionResult ForgotPassword(string username, string email)
        {
            // TÌM ĐÚNG USER CÓ KHỚP CẢ USERNAME VÀ EMAIL
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Email == email);

            if (user != null)
            {
                // Nếu đúng, reset mật khẩu về mặc định
                string newPassword = "T1Shop@123";
                user.PasswordHash = newPassword;
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Khôi phục thành công! Mật khẩu mới của bạn là: {newPassword}";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Tên đăng nhập hoặc Email không tồn tại";
            return View();
        }
    }
}