using System.Diagnostics;
using Chingoo.Models;
using Chingoo.Services.Home;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Chingoo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService, ILogger<HomeController> logger)
        {
            _homeService = homeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return View(await _homeService.GetHomeViewModelAsync(User, cancellationToken));
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
