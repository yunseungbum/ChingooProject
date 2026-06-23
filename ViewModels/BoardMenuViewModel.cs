namespace Chingoo.ViewModels
{
    public class BoardMenuViewModel
    {
        public string? NickName { get; set; }
        public string? Region { get; set; }
        public int? SoccerTemperature { get; set; }

        public bool IsLogin { get; set; }

        public string[] Days { get; set; } = Array.Empty<string>();

        public string[] MatchRegions { get; set; } = Array.Empty<string>();

        public string[] StadiumRegions { get; set; } = Array.Empty<string>();

        public string[] Times { get; set; } = Array.Empty<string>();
    }
}
