using Chingoo.Data;
using Chingoo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    public class NoticeController : Controller
    {
        private readonly AppDbContext _db;

        public NoticeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var notices = await _db.Notices
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(notices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var notice = await _db.Notices.FirstOrDefaultAsync(x => x.Id == id);

            if (notice == null)
            {
                return NotFound();
            }

            return View(notice);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice)
        {
            if (User.Identity?.Name != "admin")
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(notice);
            }

            notice.CreatedAt = DateTime.Now;

            _db.Notices.Add(notice);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}