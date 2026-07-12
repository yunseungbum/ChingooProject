namespace Chingoo.ViewModels
{
    public class YoutubeVideoViewModel
    {
        public string ChannelName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }
}