using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace T1EsportsWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class TeamsController : Controller
    {
        private readonly T1StatDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const int PageSize = 15;

        public TeamsController(T1StatDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Teams
        public async Task<IActionResult> Index(int? page, string searchName, string region, bool? hasLogo)
        {
            int pageNumber = page ?? 1;
            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(t => t.Name.Contains(searchName));

            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(t => t.Region == region);

            if (hasLogo.HasValue)
            {
                if (hasLogo.Value)
                    query = query.Where(t => !string.IsNullOrEmpty(t.LogoUrl));
                else
                    query = query.Where(t => string.IsNullOrEmpty(t.LogoUrl));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Name)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchName = searchName;
            ViewBag.Region = region;
            ViewBag.HasLogo = hasLogo;

            return View(items);
        }

        // GET: Admin/Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FirstOrDefaultAsync(m => m.IdTeam == id);
            if (team == null) return NotFound();
            return View(team);
        }

        // GET: Admin/Teams/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, Team team, IFormFile? logoFile)
        {
            // Kiểm tra tên đội đã tồn tại chưa
            var existing = await _context.Teams.FirstOrDefaultAsync(t => t.Name == team.Name);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Tên đội đã tồn tại. Vui lòng chọn tên khác.");
                return View(team);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    team.LogoUrl = await SaveImageAsync(logoFile);
                    _context.Add(team);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Create: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(team);
                }
            }
            return View(team);
        }

        // GET: Admin/Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();
            return View(team);
        }

        // POST: Admin/Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team, IFormFile? logoFile)
        {
            if (id != team.IdTeam) return NotFound();

            // Kiểm tra tên đội đã tồn tại ở bản ghi khác chưa
            var existing = await _context.Teams.FirstOrDefaultAsync(t => t.Name == team.Name && t.IdTeam != team.IdTeam);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Tên đội đã tồn tại. Vui lòng chọn tên khác.");
                return View(team);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    team.LogoUrl = await SaveImageAsync(logoFile, team.LogoUrl);
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.IdTeam)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Edit: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(team);
                }
            }
            return View(team);
        }

        // GET: Admin/Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var team = await _context.Teams.FirstOrDefaultAsync(m => m.IdTeam == id);
            if (team == null) return NotFound();
            return View(team);
        }

        // POST: Admin/Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                DeleteImage(team.LogoUrl);
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.IdTeam == id);
        }

        #region Image Helpers

        private async Task<string?> SaveImageAsync(IFormFile? imageFile, string? existingPath = null)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    Console.WriteLine("SaveImageAsync: Không có file ảnh.");
                    return existingPath;
                }

                Console.WriteLine($"SaveImageAsync: Bắt đầu xử lý file {imageFile.FileName}, kích thước {imageFile.Length} bytes.");

                // Kiểm tra WebRootPath
                if (string.IsNullOrEmpty(_hostEnvironment.WebRootPath))
                {
                    var error = "SaveImageAsync: WebRootPath is null! Kiểm tra lại cấu hình hosting.";
                    Console.WriteLine(error);
                    throw new InvalidOperationException(error);
                }

                // Tạo thư mục nếu chưa tồn tại
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "teams");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    Console.WriteLine($"SaveImageAsync: Đã tạo thư mục {uploadsFolder}");
                }

                // Kiểm tra phần mở rộng file
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException("Chỉ chấp nhận file ảnh định dạng: .jpg, .jpeg, .png, .gif, .webp");
                }

                // Tạo tên file duy nhất
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Console.WriteLine($"SaveImageAsync: Đường dẫn file sẽ lưu: {filePath}");

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                Console.WriteLine("SaveImageAsync: Lưu file thành công.");

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(existingPath))
                {
                    var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                        Console.WriteLine($"SaveImageAsync: Đã xóa file cũ {oldFilePath}");
                    }
                    else
                    {
                        Console.WriteLine($"SaveImageAsync: File cũ không tồn tại: {oldFilePath}");
                    }
                }

                return "/images/teams/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveImageAsync: LỖI - {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw; // Ném lại để action bắt được và xử lý
            }
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            try
            {
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"DeleteImage: Đã xóa file {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteImage: Lỗi khi xóa file {imagePath} - {ex.Message}");
            }
        }

        #endregion
    }
}