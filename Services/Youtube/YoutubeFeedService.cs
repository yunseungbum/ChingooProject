using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Chingoo.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Chingoo.Services.Youtube
{
    public class YoutubeFeedService : IYoutubeFeedService
    {
        private const string CacheKey = "youtube-latest-videos";
        private const string PlaylistFeedUrl = "https://www.youtube.com/feeds/videos.xml?playlist_id=PLy6jOgn2yRYTat_QoS1uMDUS_6orRasDS";
        private const string JkArtSoccerVideosUrl = "https://www.youtube.com/@JKartsoccer/videos";
        private const string GoAleVideosUrl = "https://www.youtube.com/@GoAle/videos";

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<YoutubeFeedService> _logger;

        public YoutubeFeedService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<YoutubeFeedService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<List<YoutubeVideoViewModel>> GetLatestVideosAsync(CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(CacheKey, out List<YoutubeVideoViewModel>? cachedVideos))
            {
                return cachedVideos ?? new List<YoutubeVideoViewModel>();
            }

            try
            {
                var videos = new List<YoutubeVideoViewModel>();
                var playlistVideo = await GetPlaylistLatestVideoAsync(cancellationToken);

                if (playlistVideo != null)
                {
                    videos.Add(playlistVideo);
                }

                var jkArtSoccerVideo = await GetChannelLatestVideoAsync(
                    JkArtSoccerVideosUrl,
                    "JKartsoccer",
                    cancellationToken);

                if (jkArtSoccerVideo != null)
                {
                    videos.Add(jkArtSoccerVideo);
                }

                var goAleVideo = await GetChannelLatestVideoAsync(
                    GoAleVideosUrl,
                    "고알레 유튜브",
                    cancellationToken);

                if (goAleVideo != null)
                {
                    videos.Add(goAleVideo);
                }

                if (videos.Any())
                {
                    _memoryCache.Set(CacheKey, videos, TimeSpan.FromMinutes(30));
                }

                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load latest YouTube videos.");
                return new List<YoutubeVideoViewModel>();
            }
        }

        private async Task<YoutubeVideoViewModel?> GetPlaylistLatestVideoAsync(CancellationToken cancellationToken)
        {
            try
            {
                var xml = await _httpClient.GetStringAsync(PlaylistFeedUrl, cancellationToken);
                return ParseLatestVideo(xml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load latest YouTube playlist video.");
                return null;
            }
        }

        private async Task<YoutubeVideoViewModel?> GetChannelLatestVideoAsync(
            string videosUrl,
            string channelName,
            CancellationToken cancellationToken)
        {
            try
            {
                var html = await _httpClient.GetStringAsync(videosUrl, cancellationToken);
                return ParseLatestVideoFromHtml(html, channelName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load latest YouTube channel video. ChannelName: {ChannelName}", channelName);
                return null;
            }
        }

        private static YoutubeVideoViewModel? ParseLatestVideo(string xml)
        {
            var document = XDocument.Parse(xml);

            XNamespace atom = "http://www.w3.org/2005/Atom";
            XNamespace media = "http://search.yahoo.com/mrss/";

            var entry = document.Root?
                .Elements(atom + "entry")
                .FirstOrDefault();

            if (entry == null)
            {
                return null;
            }

            var title = entry.Element(atom + "title")?.Value ?? string.Empty;
            var videoUrl = entry.Element(atom + "link")?.Attribute("href")?.Value ?? string.Empty;
            var thumbnailUrl = entry.Element(media + "group")?
                .Element(media + "thumbnail")?
                .Attribute("url")?
                .Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(videoUrl) ||
                string.IsNullOrWhiteSpace(thumbnailUrl))
            {
                return null;
            }

            return new YoutubeVideoViewModel
            {
                ChannelName = document.Root?.Element(atom + "title")?.Value ?? "YouTube",
                Title = title,
                VideoUrl = videoUrl,
                ThumbnailUrl = thumbnailUrl,
                PublishedAt = DateTime.TryParse(entry.Element(atom + "published")?.Value, out var publishedAt)
                    ? publishedAt
                    : DateTime.Now
            };
        }

        private static YoutubeVideoViewModel? ParseLatestVideoFromHtml(string html, string channelName)
        {
            foreach (var videoId in FindVideoIds(html))
            {
                var nearbyHtml = GetNearbyHtml(html, videoId);

                if (IsMembersOnlyVideo(nearbyHtml))
                {
                    continue;
                }

                var rendererHtml = GetVideoRendererHtml(html, videoId);
                var title = FindVideoTitle(rendererHtml);

                if (string.IsNullOrWhiteSpace(title))
                {
                    title = FindVideoTitle(nearbyHtml);
                }

                return new YoutubeVideoViewModel
                {
                    ChannelName = channelName,
                    Title = title,
                    VideoUrl = $"https://www.youtube.com/watch?v={videoId}",
                    ThumbnailUrl = FindThumbnailUrl(rendererHtml + nearbyHtml, videoId),
                    PublishedAt = DateTime.Now
                };
            }

            return null;
        }

        private static IEnumerable<string> FindVideoIds(string html)
        {
            return Regex
                .Matches(html, "\"videoId\"\\s*:\\s*\"(?<id>[0-9A-Za-z_-]{11})\"")
                .Select(match => match.Groups["id"].Value)
                .Distinct(StringComparer.Ordinal);
        }

        private static string GetNearbyHtml(string html, string videoId)
        {
            var index = html.IndexOf(videoId, StringComparison.Ordinal);

            if (index < 0)
            {
                return html;
            }

            var start = Math.Max(0, index - 1500);
            var length = Math.Min(html.Length - start, 6000);

            return html.Substring(start, length);
        }

        private static string GetVideoRendererHtml(string html, string videoId)
        {
            var videoIndex = html.IndexOf($"\"videoId\":\"{videoId}\"", StringComparison.Ordinal);

            if (videoIndex < 0)
            {
                return string.Empty;
            }

            var rendererStart = html.LastIndexOf("\"videoRenderer\"", videoIndex, StringComparison.Ordinal);

            if (rendererStart < 0)
            {
                rendererStart = Math.Max(0, videoIndex - 3000);
            }

            var length = Math.Min(html.Length - rendererStart, 9000);
            return html.Substring(rendererStart, length);
        }

        private static bool IsMembersOnlyVideo(string html)
        {
            var membersOnlyMarkers = new[]
            {
                "members-only",
                "Members only",
                "Join this channel",
                "회원 전용",
                "회원전용",
                "멤버십 전용"
            };

            return membersOnlyMarkers.Any(marker =>
                html.Contains(marker, StringComparison.OrdinalIgnoreCase));
        }

        private static string FindVideoTitle(string html)
        {
            var patterns = new[]
            {
                "\"title\"\\s*:\\s*\\{\\s*\"runs\"\\s*:\\s*\\[\\s*\\{\\s*\"text\"\\s*:\\s*\"(?<title>.*?)\"",
                "\"title\"\\s*:\\s*\\{\\s*\"simpleText\"\\s*:\\s*\"(?<title>.*?)\"",
                "\"accessibility\"\\s*:\\s*\\{.*?\"label\"\\s*:\\s*\"(?<title>.*?)\""
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.Singleline);

                if (match.Success)
                {
                    return WebUtility.HtmlDecode(Regex.Unescape(match.Groups["title"].Value));
                }
            }

            return string.Empty;
        }

        private static string FindThumbnailUrl(string html, string videoId)
        {
            var match = Regex.Match(
                html,
                "\"url\"\\s*:\\s*\"(?<url>https://i\\.ytimg\\.com/vi/" + Regex.Escape(videoId) + "/[^\"\\\\]+)\"");

            if (match.Success)
            {
                return WebUtility.HtmlDecode(Regex.Unescape(match.Groups["url"].Value));
            }

            return $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";
        }
    }
}
