using System.Diagnostics;
using Chingoo.Models;
using Microsoft.AspNetCore.Mvc;
using Chingoo.ViewModels;

namespace Chingoo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
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
