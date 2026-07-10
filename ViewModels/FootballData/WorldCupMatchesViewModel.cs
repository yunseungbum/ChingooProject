namespace Chingoo.ViewModels.FootballData
{
    public class WorldCupMatchesViewModel
    {
        public string CompetitionName { get; set; } = "FIFA World Cup";
        public string CompetitionCode { get; set; } = "WC";
        public string? CompetitionEmblem { get; set; }
        public int Season { get; set; }
        public int TotalCount { get; set; }
        public int PlayedCount { get; set; }
        public List<WorldCupMatchViewModel> FinishedMatches { get; set; } = new();
        public List<WorldCupMatchViewModel> UpcomingMatches { get; set; } = new();
    }

    public class WorldCupMatchViewModel
    {
        public int Id { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string? Group { get; set; }
        public int? Matchday { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public DateTime UtcDate { get; set; }
        public DateTime KoreaDate { get; set; }
        public string DateText { get; set; } = string.Empty;
        public string TimeText { get; set; } = string.Empty;
        public string HomeTeamName { get; set; } = string.Empty;
        public string? HomeTeamCrest { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string? AwayTeamCrest { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public string ScoreText { get; set; } = string.Empty;
    }
}
