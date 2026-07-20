using Chingoo.Common;
using Chingoo.Data;
using Chingoo.Models;
using Chingoo.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Posts
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _db;

        public PostService(AppDbContext db)
        {
            _db = db;
        }

        public PostCreateViewModel GetCreateViewModel()
        {
            return new PostCreateViewModel
            {
                MatchDate = DateTime.Today,
                Days = BoardOptions.Days,
                Regions = BoardOptions.Regions,
                Times = BoardOptions.Times
            };
        }

        public async Task CreatePostAsync(PostCreateViewModel model, int userId)
        {
            var post = new Post
            {
                UserId = userId,
                BoardType = model.BoardType,
                DayType = model.DayType,
                Region = model.Region,
                MatchDate = GetEffectiveMatchDate(model),
                TimeSlot = model.TimeSlot,
                Title = model.Title,
                Content = model.Content
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
        }

        public List<Post> GetPosts(string boardType, string day, string region, string time)
        {
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

            if (!string.IsNullOrEmpty(time))
            {
                query = query.Where(x => x.TimeSlot == time);
            }

            return query.OrderByDescending(x => x.CreatedAt).ToList();
        }

        public async Task<Post?> GetPostDetailsAsync(int id)
        {
            return await _db.Posts
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CanWriteReplyAsync(int postId, int userId)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);

            return post != null && post.UserId == userId;
        }

        public async Task<PostCreateViewModel?> GetEditViewModelAsync(int id, int userId)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (post == null)
            {
                return null;
            }

            return new PostCreateViewModel
            {
                BoardType = post.BoardType,
                DayType = post.DayType,
                Region = post.Region,
                MatchDate = post.MatchDate,
                TimeSlot = post.TimeSlot,
                Title = post.Title,
                Content = post.Content,
                Days = BoardOptions.Days,
                Regions = BoardOptions.Regions,
                Times = BoardOptions.Times
            };
        }

        public async Task<bool> UpdatePostAsync(int id, PostCreateViewModel model, int userId)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (post == null)
            {
                return false;
            }

            post.BoardType = model.BoardType;
            post.DayType = model.DayType;
            post.Region = model.Region;
            post.MatchDate = GetEffectiveMatchDate(model);
            post.TimeSlot = model.TimeSlot;
            post.Title = model.Title;
            post.Content = model.Content;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int id, int userId, bool isAdmin)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.UserId == userId));

            if (post == null)
            {
                return false;
            }

            var comments = await _db.Comments
                .Where(x => x.BoardType == "Post" && x.BoardId == id)
                .ToListAsync();

            _db.Comments.RemoveRange(comments);
            _db.Posts.Remove(post);

            await _db.SaveChangesAsync();
            return true;
        }

        private static DateTime GetEffectiveMatchDate(PostCreateViewModel model)
        {
            return model.BoardType == "팀원 모집" ? DateTime.Today : model.MatchDate;
        }

        public PostManageViewModel GetManageViewModel(int userId, string filter)
        {
            filter = string.IsNullOrWhiteSpace(filter) ? "전체" : filter;

            var postItems = _db.Posts
                .Where(x => x.UserId == userId)
                .Select(x => new PostManageListItemViewModel
                {
                    Id = x.Id,
                    SourceType = "Post",
                    BoardType = x.BoardType,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    Region = x.Region,
                    DayType = x.DayType,
                    TimeSlot = x.TimeSlot,
                    MatchDate = x.BoardType == "팀원 모집" ? null : x.MatchDate
                })
                .ToList();

            var communityItems = _db.CommunityPosts
                .Where(x => x.UserId == userId)
                .Select(x => new PostManageListItemViewModel
                {
                    Id = x.Id,
                    SourceType = "Community",
                    BoardType = "커뮤니티",
                    Title = x.Title,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            var items = postItems
                .Concat(communityItems)
                .Where(x => filter == "전체" || x.BoardType == filter)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return new PostManageViewModel
            {
                Filter = filter,
                Items = items
            };
        }
    }
}
