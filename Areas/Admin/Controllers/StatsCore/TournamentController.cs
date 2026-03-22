using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace T1EsportsWeb.Areas.Admin.Controllers.StatsCore
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Staff")]
    public class TournamentsController : Controller
    {
        private readonly T1StatDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const int PageSize = 15;

        public TournamentsController(T1StatDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Tournaments
        public async Task<IActionResult> Index(int? page, string searchName, int? year, string region, string isT1winner)
        {
            int pageNumber = page ?? 1;
            var query = _context.Tournaments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(t => t.Name.Contains(searchName));

            if (year.HasValue)
                query = query.Where(t => t.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(t => t.Region == region);

            // Ánh xạ từ UI (Win/Loss) sang DB (YES/NO)
            if (!string.IsNullOrWhiteSpace(isT1winner))
            {
                if (isT1winner == "Win")
                    query = query.Where(t => t.IsT1winner == "YES");
                else if (isT1winner == "Loss")
                    query = query.Where(t => t.IsT1winner == "NO");
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
            ViewBag.Year = year;
            ViewBag.Region = region;
            ViewBag.IsT1winner = isT1winner; // giữ lại giá trị UI để hiển thị dropdown

            return View(items);
        }

        // GET: Admin/Tournaments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(m => m.IdTournament == id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        // GET: Admin/Tournaments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Tournaments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament, IFormFile? imageFile)
        {
            // Kiểm tra tên giải đã tồn tại chưa
            var existing = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == tournament.Name && t.Year == tournament.Year);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Giải đấu với tên và năm này đã tồn tại.");
                return View(tournament);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Không có ảnh upload cho Tournament? Nếu có, xử lý tương tự
                    // tournament.ImageUrl = await SaveImageAsync(imageFile, "tournaments");
                    _context.Add(tournament);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Create: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(tournament);
                }
            }
            return View(tournament);
        }

        // GET: Admin/Tournaments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        // POST: Admin/Tournaments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tournament tournament, IFormFile? imageFile)
        {
            if (id != tournament.IdTournament) return NotFound();

            // Kiểm tra trùng tên + năm (trừ bản ghi hiện tại)
            var existing = await _context.Tournaments.FirstOrDefaultAsync(t => t.Name == tournament.Name && t.Year == tournament.Year && t.IdTournament != tournament.IdTournament);
            if (existing != null)
            {
                ModelState.AddModelError("Name", "Giải đấu với tên và năm này đã tồn tại.");
                return View(tournament);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý ảnh nếu có
                    // tournament.ImageUrl = await SaveImageAsync(imageFile, "tournaments", tournament.ImageUrl);
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.IdTournament)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi trong Edit: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                    return View(tournament);
                }
            }
            return View(tournament);
        }

        // GET: Admin/Tournaments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(m => m.IdTournament == id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        // POST: Admin/Tournaments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                // Xóa ảnh nếu có
                // DeleteImage(tournament.ImageUrl, "tournaments");
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TournamentExists(int id)
        {
            return _context.Tournaments.Any(e => e.IdTournament == id);
        }

        // Hàm xử lý ảnh (nếu cần) - tương tự TeamsController, chỉ khác thư mục
        // Có thể dùng chung một helper, nhưng để đơn giản, copy từ TeamsController và sửa đường dẫn
    }
}