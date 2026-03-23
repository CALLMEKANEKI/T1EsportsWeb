using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Areas.Admin.Controllers.StatsCore
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class PlayersController : Controller
    {
        private readonly T1StatDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const int PageSize = 15;

        public PlayersController(T1StatDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Players
        // GET: Admin/Players

        private async Task<List<SelectListItem>> GetTeamsSelectListAsync(int? selectedTeamId = null)
        {
            var teams = await _context.Teams.OrderBy(t => t.Name).ToListAsync();
            return teams.Select(t => new SelectListItem
            {
                Value = t.IdTeam.ToString(),
                Text = t.Name,
                Selected = selectedTeamId.HasValue && t.IdTeam == selectedTeamId.Value
            }).ToList();
        }
        public async Task<IActionResult> Index(int? page, string searchIngame, bool? hasImage, string position, string country, int? teamId)
        {
            int pageNumber = page ?? 1;

            // 🎯 MẸO: Nếu là lần đầu tiên truy cập (không có tham số), tự động lọc đội T1
            if (Request.Query.Count == 0)
            {
                var t1Team = await _context.Teams.FirstOrDefaultAsync(t => t.Name.Contains("T1"));
                if (t1Team != null)
                {
                    teamId = t1Team.IdTeam;
                }
            }

            var query = _context.Players.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchIngame))
                query = query.Where(p => p.IngameName.Contains(searchIngame));

            if (hasImage.HasValue)
            {
                if (hasImage.Value)
                    query = query.Where(p => !string.IsNullOrEmpty(p.PhotoUrl));
                else
                    query = query.Where(p => string.IsNullOrEmpty(p.PhotoUrl));
            }

            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(p => p.Position == position);

            if (!string.IsNullOrWhiteSpace(country))
                query = query.Where(p => p.Country != null && p.Country.Contains(country));

            // Lọc theo team
            if (teamId.HasValue)
                query = query.Where(p => p.TeamId == teamId.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .Include(p => p.Team) // Include để lấy thông tin team
                .OrderBy(p => p.IngameName)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.SearchIngame = searchIngame;
            ViewBag.HasImage = hasImage;
            ViewBag.Position = position;
            ViewBag.Country = country;
            ViewBag.TeamId = teamId;

            // Lấy danh sách team để hiển thị dropdown (chỉ các team có tuyển thủ)
            var teams = await _context.Teams
                .Where(t => _context.Players.Any(p => p.TeamId == t.IdTeam))
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.Teams = teams;

            return View(items);
        }

        // GET: Admin/Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var player = await _context.Players
                        .Include(p => p.Team)
                        .FirstOrDefaultAsync(m => m.IdPlayer == id);
            if (player == null) return NotFound();
            return View(player);
        }

        // GET: Admin/Players/Create
        public IActionResult Create()
        {
            ViewBag.Teams = GetTeamsSelectListAsync().Result;
            return View();
        }

        // POST: Admin/Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Player player, IFormFile? photoFile)
        {
            // Kiểm tra tên ingame đã tồn tại chưa
            var existing = await _context.Players.FirstOrDefaultAsync(p => p.IngameName == player.IngameName);
            if (existing != null)
            {
                ModelState.AddModelError("IngameName", "Tên ingame đã tồn tại.");
                return View(player);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    player.PhotoUrl = await SaveImageAsync(photoFile, "players");
                    _context.Add(player);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Create: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    ViewBag.Teams = await GetTeamsSelectListAsync(player.TeamId);
                    return View(player);
                }
            }
            ViewBag.Teams = await GetTeamsSelectListAsync(player.TeamId);
            return View(player);
        }

        // GET: Admin/Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var player = await _context.Players
                        .Include(p => p.Team)
                        .FirstOrDefaultAsync(p => p.IdPlayer == id);
            if (player == null) return NotFound();
            return View(player);
        }

        // POST: Admin/Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Player player, IFormFile? photoFile)
        {
            if (id != player.IdPlayer) return NotFound();

            // Kiểm tra trùng tên ingame (trừ bản ghi hiện tại)
            var existing = await _context.Players.FirstOrDefaultAsync(p => p.IngameName == player.IngameName && p.IdPlayer != player.IdPlayer);
            if (existing != null)
            {
                ModelState.AddModelError("IngameName", "Tên ingame đã tồn tại.");
                ViewBag.Teams = await GetTeamsSelectListAsync(player.TeamId);
                return View(player);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    player.PhotoUrl = await SaveImageAsync(photoFile, "players", player.PhotoUrl);
                    _context.Update(player);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.IdPlayer)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Edit: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    ViewBag.Teams = await GetTeamsSelectListAsync(player.TeamId);
                    return View(player);
                }
            }
            ViewBag.Teams = await GetTeamsSelectListAsync(player.TeamId);
            return View(player);
        }

        // GET: Admin/Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var player = await _context.Players
                        .Include(p => p.Team)
                        .FirstOrDefaultAsync(m => m.IdPlayer == id);
            if (player == null) return NotFound();
            return View(player);
        }

        // POST: Admin/Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                DeleteImage(player.PhotoUrl, "players");
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.IdPlayer == id);
        }

        #region Image Helpers

        private async Task<string?> SaveImageAsync(IFormFile? imageFile, string folder, string? existingPath = null)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return existingPath;
                }

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