using Chingoo.Models;

namespace Chingoo.Services.Notices
{
    public interface INoticeService
    {
        Task<List<Notice>> GetNoticesAsync();
        Task<Notice?> GetNoticeAsync(int id);
        Task<bool> NoticeExistsAsync(int id);
        Task CreateNoticeAsync(Notice notice);
    }
}
