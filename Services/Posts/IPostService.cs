using Chingoo.Models;
using Chingoo.ViewModels;

namespace Chingoo.Services.Posts
{
    public interface IPostService
    {
        PostCreateViewModel GetCreateViewModel();
        Task CreatePostAsync(PostCreateViewModel model, int userId);
        List<Post> GetPosts(string boardType, string day, string region, string time);
        Task<Post?> GetPostDetailsAsync(int id);
        Task<bool> CanWriteReplyAsync(int postId, int userId);
        Task<PostCreateViewModel?> GetEditViewModelAsync(int id, int userId);
        Task<bool> UpdatePostAsync(int id, PostCreateViewModel model, int userId);
        Task<bool> DeletePostAsync(int id, int userId);
        PostManageViewModel GetManageViewModel(int userId, string filter);
    }
}
