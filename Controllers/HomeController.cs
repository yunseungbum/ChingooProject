using System.Diagnostics;
using Chingoo.Models;
using Microsoft.AspNetCore.Mvc;
using Chingoo.ViewModels;
using System.Security.Claims;
using Chingoo.Data;
using Chingoo.Common;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _db;
        public HomeController(AppDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                BoardMenu = new BoardMenuViewModel
                {
                    Days = BoardOptions.Days,
                    MatchRegions = BoardOptions.Regions,
                    StadiumRegions = new[] { "서울", "인천, 부천", "경기" },
                    Times = BoardOptions.Times
                },
                Notices = _db.Notices.OrderByDescending(x => x.CreatedAt).Take(3).ToList(),
                CommunityPosts = _db.CommunityPosts.OrderByDescending(x => x.CreatedAt).Take(5).ToList()
            };

            if (User.Identity.IsAuthenticated)
            {
                var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdValue, out int userId))
                {
                    var user = _db.Users.FirstOrDefault(x => x.Id == userId);

                    if (user != null)
                    {
                        model.BoardMenu.IsLogin = true;
                        model.BoardMenu.NickName = user.TeamName;
                        model.BoardMenu.Region = user.Region;
                        model.BoardMenu.SoccerTemperature = user.SoccerTemperature;
                    }
                }
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
