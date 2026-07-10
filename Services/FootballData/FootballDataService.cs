using System.Net.Http.Headers;
using System.Text.Json;
using Chingoo.ViewModels.FootballData;
using Microsoft.Extensions.Options;

namespace Chingoo.Services.FootballData
{
    public class FootballDataService : IFootballDataService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly FootballDataOptions _options;

        public FootballDataService(HttpClient httpClient, IOptions<FootballDataOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<WorldCupMatchesViewModel> GetWorldCupMatchesAsync(int season = 2026, CancellationToken cancellationToken = default)
        {
            var dateFrom = new DateTime(2026, 7, 5).ToString("yyyy-MM-dd");
            var dateTo = new DateTime(2026, 7, 21).ToString("yyyy-MM-dd");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"competitions/WC/matches?season={season}&dateFrom={dateFrom}&dateTo={dateTo}");
            request.Headers.Add("X-Auth-Token", _options.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var data = await JsonSerializer.DeserializeAsync<FootballDataMatchesResponse>(responseStream, JsonOptions, cancellationToken)
                ?? new FootballDataMatchesResponse();

            var matches = data.Matches
                .OrderBy(match => match.UtcDate)
                .Select(MapMatch)
                .ToList();

            return new WorldCupMatchesViewModel
            {
                CompetitionName = data.Competition?.Name ?? "FIFA World Cup",
                CompetitionCode = data.Competition?.Code ?? "WC",
                CompetitionEmblem = data.Competition?.Emblem,
                Season = season,
                TotalCount = data.ResultSet?.Count ?? matches.Count,
                PlayedCount = data.ResultSet?.Played ?? matches.Count(match => match.Status == "FINISHED"),
                FinishedMatches = matches.Where(match => match.Status == "FINISHED").ToList(),
                UpcomingMatches = matches.Where(match => match.Status != "FINISHED").ToList()
            };
        }

        private static WorldCupMatchViewModel MapMatch(FootballDataMatch match)
        {
            var koreaDate = DateTime.SpecifyKind(match.UtcDate, DateTimeKind.Utc).AddHours(9);
            var homeScore = match.Score?.FullTime?.Home;
            var awayScore = match.Score?.FullTime?.Away;

            return new WorldCupMatchViewModel
            {
                Id = match.Id,
                Stage = FormatStage(match.Stage),
                Group = FormatGroup(match.Group),
                Matchday = match.Matchday,
                Status = match.Status ?? string.Empty,
                StatusText = FormatStatus(match.Status),
                UtcDate = match.UtcDate,
                KoreaDate = koreaDate,
                DateText = koreaDate.ToString("MM.dd"),
                TimeText = koreaDate.ToString("HH:mm"),
                HomeTeamName = GetTeamName(match.HomeTeam),
                HomeTeamCrest = match.HomeTeam?.Crest,
                AwayTeamName = GetTeamName(match.AwayTeam),
                AwayTeamCrest = match.AwayTeam?.Crest,
                HomeScore = homeScore,
                AwayScore = awayScore,
                ScoreText = homeScore.HasValue && awayScore.HasValue ? $"{homeScore} : {awayScore}" : "vs"
            };
        }

        private static string GetTeamName(FootballDataTeam? team)
        {
            return team?.ShortName ?? team?.Name ?? team?.Tla ?? "TBD";
        }

        private static string FormatStatus(string? status)
        {
            return status switch
            {
                "FINISHED" => "\uC885\uB8CC",
                "TIMED" => "\uC608\uC815",
                "SCHEDULED" => "\uC608\uC815",
                "IN_PLAY" => "\uC9C4\uD589\uC911",
                "PAUSED" => "\uD558\uD504\uD0C0\uC784",
                "POSTPONED" => "\uC5F0\uAE30",
                "CANCELLED" => "\uCDE8\uC18C",
                _ => status ?? string.Empty
            };
        }

        private static string FormatStage(string? stage)
        {
            return stage switch
            {
                "GROUP_STAGE" => "\uC870\uBCC4\uB9AC\uADF8",
                "LAST_16" => "16\uAC15",
                "QUARTER_FINALS" => "8\uAC15",
                "SEMI_FINALS" => "4\uAC15",
                "THIRD_PLACE" => "3/4\uC704\uC804",
                "FINAL" => "\uACB0\uC2B9",
                _ => stage?.Replace('_', ' ') ?? string.Empty
            };
        }

        private static string? FormatGroup(string? group)
        {
            return string.IsNullOrWhiteSpace(group) ? null : group.Replace("GROUP_", "Group ");
        }
    }
}





