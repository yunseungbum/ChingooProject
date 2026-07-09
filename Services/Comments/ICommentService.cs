using Chingoo.Models;

namespace Chingoo.Services.Comments
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsAsync(string boardType, int boardId);
        Task<bool> ParentCommentExistsAsync(string boardType, int boardId, int parentCommentId);
        Task CreateCommentAsync(string boardType, int boardId, int userId, string content, int? parentCommentId);
    }
}
