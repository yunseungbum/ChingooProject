using Chingoo.Data;
using Chingoo.Models;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Comment>> GetCommentsAsync(string boardType, int boardId)
        {
            return await _db.Comments
                .Include(x => x.User)
                .Include(x => x.Replies)
                    .ThenInclude(x => x.User)
                .Where(x => x.BoardType == boardType && x.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<bool> ParentCommentExistsAsync(string boardType, int boardId, int parentCommentId)
        {
            return await _db.Comments
                .AnyAsync(x => x.Id == parentCommentId && x.BoardType == boardType && x.BoardId == boardId);
        }

        public async Task CreateCommentAsync(string boardType, int boardId, int userId, string content, int? parentCommentId)
        {
            var comment = new Comment
            {
                BoardType = boardType,
                BoardId = boardId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
        }
    }
}
