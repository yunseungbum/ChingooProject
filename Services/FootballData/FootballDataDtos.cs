using System.Text.Json.Serialization;

namespace Chingoo.Services.FootballData
{
    public class FootballDataMatchesResponse
    {
        [JsonPropertyName("resultSet")]
        public FootballDataResultSet? ResultSet { get; set; }

        [JsonPropertyName("competition")]
        public FootballDataCompetition? Competition { get; set; }

        [JsonPropertyName("matches")]
        public List<FootballDataMatch> Matches { get; set; } = new();
    }

    public class FootballDataResultSet
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("played")]
        public int Played { get; set; }
    }

    public class FootballDataCompetition
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("emblem")]
        public string? Emblem { get; set; }
    }

    public class FootballDataMatch
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("utcDate")]
        public DateTime UtcDate { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("matchday")]
        public int? Matchday { get; set; }

        [JsonPropertyName("stage")]
        public string? Stage { get; set; }

        [JsonPropertyName("group")]
        public string? Group { get; set; }

        [JsonPropertyName("homeTeam")]
        public FootballDataTeam? HomeTeam { get; set; }

        [JsonPropertyName("awayTeam")]
        public FootballDataTeam? AwayTeam { get; set; }

        [JsonPropertyName("score")]
        public FootballDataScore? Score { get; set; }
    }

    public class FootballDataTeam
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("shortName")]
        public string? ShortName { get; set; }

        [JsonPropertyName("tla")]
        public string? Tla { get; set; }

        [JsonPropertyName("crest")]
        public string? Crest { get; set; }
    }

    public class FootballDataScore
    {
        [JsonPropertyName("fullTime")]
        public FootballDataFullTimeScore? FullTime { get; set; }
    }

    public class FootballDataFullTimeScore
    {
        [JsonPropertyName("home")]
        public int? Home { get; set; }

        [JsonPropertyName("away")]
        public int? Away { get; set; }
    }
}
