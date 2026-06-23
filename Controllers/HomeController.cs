using System.Diagnostics;
using Chingoo.Models;
using Microsoft.AspNetCore.Mvc;
using Chingoo.ViewModels;
using System.Security.Claims;
using Chingoo.Data;

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
                    Days = new[]
                    {
                        "평일",
                        "주말"
                    },

                    MatchRegions = new[]
                    {
                        "서울 북부",
                        "서울 남부",
                        "인천, 부천",
                        "경기 북부",
                        "경기 남부"
                    },

                    StadiumRegions = new[]
                    {
                        "서울",
                        "인천, 부천",
                        "경기"
                    },

                    Times = new[]
                    {
                        "06:00~08:00",
                        "07:00~09:00",
                        "08:00~10:00",
                        "09:00~11:00",
                        "10:00~12:00",
                        "11:00~13:00",
                        "12:00~14:00",
                        "13:00~15:00",
                        "14:00~16:00",
                        "15:00~17:00",
                        "16:00~18:00",
                        "17:00~19:00",
                        "18:00~20:00",
                        "19:00~21:00",
                        "20:00~22:00",
                        "21:00~23:00"
                    }
                }
            };

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var user = _db.Users.FirstOrDefault(x => x.Id == int.Parse(userId));

                model.BoardMenu.IsLogin = true;
                model.BoardMenu.NickName = user.TeamName;
                model.BoardMenu.Region = user.Region;
                model.BoardMenu.SoccerTemperature = user.SoccerTemperature;
            }
            else
            {
                model.BoardMenu.IsLogin = false;
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
