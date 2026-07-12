using Chingoo.ViewModels;

namespace Chingoo.Services.Youtube
{
    public interface IYoutubeFeedService
    {
        Task<List<YoutubeVideoViewModel>> GetLatestVideosAsync(CancellationToken cancellationToken = default);
    }
}