using Chingoo.Common;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chingoo.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var user = HttpContext.User;

            var model = new SidebarViewModel
            {
                IsLogin = user.Identity?.IsAuthenticated ?? false,

                NickName = user.FindFirstValue(ClaimTypes.Name),
                Region = user.FindFirstValue("Region") ?? "미지정",
                SoccerTemperature = 36,

                Days = BoardOptions.Days,
                Regions = BoardOptions.Regions
            };

            return View(model);
        }
    }
}