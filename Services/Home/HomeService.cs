using System.Security.Claims;
using Chingoo.Common;
using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _db;

        public HomeService(AppDbContext db)
        {
            _db = db;
        }

        public HomeViewModel GetHomeViewModel(ClaimsPrincipal principal)
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
                CommunityPosts = _db.CommunityPosts.OrderByDescending(x => x.CreatedAt).Take(5).ToList(),
                RecommendedMatches = GetRecommendedPosts("축구 매치", null),
                RecommendedMercenaries = GetRecommendedPosts("용병 모집", null)
            };

            var currentUser = GetCurrentUser(principal);

            if (currentUser == null)
            {
                return model;
            }

            model.BoardMenu.IsLogin = true;
            model.BoardMenu.NickName = currentUser.TeamName;
            model.BoardMenu.Region = currentUser.Region;
            model.BoardMenu.SoccerTemperature = currentUser.SoccerTemperature;
            model.RecommendedMatches = GetRecommendedPosts("축구 매치", currentUser);
            model.RecommendedMercenaries = GetRecommendedPosts("용병 모집", currentUser);

            return model;
        }

        private User? GetCurrentUser(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            return _db.Users.FirstOrDefault(x => x.Id == userId);
        }

        private List<Post> GetRecommendedPosts(string boardType, User? user)
        {
            var query = _db.Posts
                .Include(x => x.User)
                .Where(x => x.BoardType == boardType);

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.Region))
                {
                    query = query.Where(x => x.Region == user.Region);
                }

                if (!string.IsNullOrWhiteSpace(user.PreferredDayType) && user.PreferredDayType != "상관없음")
                {
                    query = query.Where(x => x.DayType == user.PreferredDayType);
                }

                if (!string.IsNullOrWhiteSpace(user.PreferredTimeSlot) && user.PreferredTimeSlot != "상관없음")
                {
                    query = query.Where(x => x.TimeSlot == user.PreferredTimeSlot);
                }
            }

            return query
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();
        }
    }
}
