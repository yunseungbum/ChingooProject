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
                MatchDate = model.MatchDate,
                TimeSlot = model.TimeSlot,
                Title = model.Title,
                Content = model.Content
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
        }

        public List<Post> GetPosts(string boardType, string day, string region)
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
    }
}
