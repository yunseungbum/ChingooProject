using Chingoo.Models;
using Chingoo.ViewModels;

namespace Chingoo.Services.Posts
{
    public interface IPostService
    {
        PostCreateViewModel GetCreateViewModel();
        Task CreatePostAsync(PostCreateViewModel model, int userId);
        List<Post> GetPosts(string boardType, string day, string region);
        Task<Post?> GetPostDetailsAsync(int id);
        Task<bool> CanWriteReplyAsync(int postId, int userId);
    }
}
