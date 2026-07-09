using Chingoo.Models;

namespace Chingoo.Services.Communities
{
    public interface ICommunityService
    {
        Task<List<CommunityPost>> GetPostsAsync();
        Task<CommunityPost?> GetPostAsync(int id);
        Task CreatePostAsync(CommunityPost communityPost, int userId);
        Task<bool> CanWriteReplyAsync(int communityPostId, int userId);
    }
}
