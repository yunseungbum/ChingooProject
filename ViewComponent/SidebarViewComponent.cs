using Chingoo.Common;
using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chingoo.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        public SidebarViewComponent(AppDbContext db)
        {
            _db = db;
        }
        public IViewComponentResult Invoke()
        {
            var user = HttpContext.User;
            User dbuser = null;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if(int.TryParse(userId, out var id))
            {
                dbuser = _db.Users.FirstOrDefault(x => x.Id == id);
            }

            var model = new SidebarViewModel
            {
                IsLogin = user.Identity?.IsAuthenticated ?? false,

                NickName = user.FindFirstValue("TeamName"),
                Region = user.FindFirstValue("Region") ?? "미지정",
                SoccerTemperature = 36,

                Days = BoardOptions.Days,
                Regions = BoardOptions.Regions
            };

            return View(model);
        }
    }
}