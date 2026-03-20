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
    public class ChampionsController : Controller
    {
        private readonly T1StatDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const int PageSize = 15;

        public ChampionsController(T1StatDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Champions
        // GET: Admin/Champions
        public async Task<IActionResult> Index(int? page, string searchName, bool? hasImage)
        {
            int pageNumber = page ?? 1;
            var query = _context.Champions.AsQueryable();

            // Lọc theo tên
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                query = query.Where(c => c.Name.Contains(searchName));
            }

            // Lọc theo trạng thái ảnh
            if (hasImage.HasValue)
            {
                if (hasImage.Value)
                    query = query.Where(c => !string.IsNullOrEmpty(c.ImageUrl));
                else
                    query = query.Where(c => string.IsNullOrEmpty(c.ImageUrl));
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchName = searchName;
            ViewBag.HasImage = hasImage; // Giữ lại giá trị để hiển thị trên dropdown

            return View(items);
        }

        // GET: Admin/Champions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var champion = await _context.Champions.FirstOrDefaultAsync(m => m.IdChampion == id);
            if (champion == null) return NotFound();
            return View(champion);
        }

        // GET: Admin/Champions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Champions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Champion champion, IFormFile? imageFile)
        {
            // Kiểm tra tên tướng đã tồn tại chưa
            var existing = await _context.Champions.FirstOrDefaultAsync(c => c.Name == champion.Name);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Tên tướng đã tồn tại.");
                return View(champion);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    champion.ImageUrl = await SaveImageAsync(imageFile, "champions");
                    _context.Add(champion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Create: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(champion);
                }
            }
            return View(champion);
        }

        // GET: Admin/Champions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var champion = await _context.Champions.FindAsync(id);
            if (champion == null) return NotFound();
            return View(champion);
        }

        // POST: Admin/Champions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Champion champion, IFormFile? imageFile)
        {
            if (id != champion.IdChampion) return NotFound();

            // Kiểm tra trùng tên (trừ bản ghi hiện tại)
            var existing = await _context.Champions.FirstOrDefaultAsync(c => c.Name == champion.Name && c.IdChampion != champion.IdChampion);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Tên tướng đã tồn tại.");
                return View(champion);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    champion.ImageUrl = await SaveImageAsync(imageFile, "champions", champion.ImageUrl);
                    _context.Update(champion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChampionExists(champion.IdChampion)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Edit: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(champion);
                }
            }
            return View(champion);
        }

        // GET: Admin/Champions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var champion = await _context.Champions.FirstOrDefaultAsync(m => m.IdChampion == id);
            if (champion == null) return NotFound();
            return View(champion);
        }

        // POST: Admin/Champions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var champion = await _context.Champions.FindAsync(id);
            if (champion != null)
            {
                DeleteImage(champion.ImageUrl, "champions");
                _context.Champions.Remove(champion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ChampionExists(int id)
        {
            return _context.Champions.Any(e => e.IdChampion == id);
        }

        #region Image Helpers
        // (Dùng chung các hàm SaveImageAsync, DeleteImage từ PlayersController - có thể copy vào)
        private async Task<string?> SaveImageAsync(IFormFile? imageFile, string folder, string? existingPath = null)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return existingPath;

                if (string.IsNullOrEmpty(_hostEnvironment.WebRootPath))
                    throw new InvalidOperationException("WebRootPath is null");

                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(fileExtension))
                    throw new InvalidOperationException("Định dạng file không hợp lệ.");

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(existingPath))
                {
                    var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                return "/images/" + folder + "/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SaveImageAsync: {ex.Message}");
                throw;
            }
        }

        private void DeleteImage(string? imagePath, string folder)
        {
            if (string.IsNullOrEmpty(imagePath)) return;
            try
            {
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi DeleteImage: {ex.Message}");
            }
        }
        #endregion
    }
}