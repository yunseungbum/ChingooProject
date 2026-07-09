using Chingoo.Data;
using Chingoo.Models;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Communities
{
    public class CommunityService : ICommunityService
    {
        private readonly AppDbContext _db;

        public CommunityService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CommunityPost>> GetPostsAsync()
        {
            return await _db.CommunityPosts
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommunityPost?> GetPostAsync(int id)
        {
            return await _db.CommunityPosts
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreatePostAsync(CommunityPost communityPost, int userId)
        {
            communityPost.UserId = userId;
            communityPost.CreatedAt = DateTime.Now;

            _db.CommunityPosts.Add(communityPost);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CanWriteReplyAsync(int communityPostId, int userId)
        {
            var post = await _db.CommunityPosts.FirstOrDefaultAsync(x => x.Id == communityPostId);

            return post != null && post.UserId == userId;
        }
    }
}
