using System.Security.Claims;
using Chingoo.Common;
using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly AppDbContext _db;

        public PostController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new PostCreateViewModel
            {
                MatchDate = DateTime.Today,
                Days = BoardOptions.Days,
                Regions = BoardOptions.Regions,
                Times = BoardOptions.Times
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Days = BoardOptions.Days;
                model.Regions = BoardOptions.Regions;
                model.Times = BoardOptions.Times;

                return View(model);
            }

            //방어코드
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdValue))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdValue);

            Post post = new Post
            {
                UserId = userId,
                BoardType = model.BoardType,
                DayType = model.DayType,
                Region = model.Region,
                MatchDate = model.MatchDate,
                TimeSlot = model.TimeSlot,
                Title = model.Title,
                Content = model.Content
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new
            {
                boardType = model.BoardType
            });
        }

        public IActionResult Index(string boardType, string day, string region)
        {
            ViewBag.BoardType = boardType;

            var query = _db.Posts.Include(x => x.User).AsQueryable();

            if (!string.IsNullOrEmpty(boardType))
            {
                query = query.Where(x => x.BoardType == boardType);
            }

            if (!string.IsNullOrEmpty(day))
            {
                query = query.Where(x => x.DayType == day);
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(x => x.Region == region);
            }

            var posts = query.OrderByDescending(x => x.CreatedAt).ToList();

            return View(posts);
        }

        public IActionResult Details(int id)
        {
            var post = _db.Posts.Include(x => x.User).FirstOrDefault(x => x.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}