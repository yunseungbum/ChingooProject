using Chingoo.Data;
using Chingoo.Models;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Services.Notices
{
    public class NoticeService : INoticeService
    {
        private readonly AppDbContext _db;

        public NoticeService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Notice>> GetNoticesAsync()
        {
            return await _db.Notices
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notice?> GetNoticeAsync(int id)
        {
            return await _db.Notices.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> NoticeExistsAsync(int id)
        {
            return await _db.Notices.AnyAsync(x => x.Id == id);
        }

        public async Task CreateNoticeAsync(Notice notice)
        {
            notice.CreatedAt = DateTime.Now;

            _db.Notices.Add(notice);
            await _db.SaveChangesAsync();
        }
    }
}
