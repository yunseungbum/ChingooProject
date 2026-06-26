namespace Chingoo.ViewModels
{
    public class SidebarViewModel
    {
        public bool IsLogin { get; set; }

        public string? NickName { get; set; }
        public string? Region { get; set; }
        public int SoccerTemperature { get; set; }

        public string[] Days { get; set; } = [];
        public string[] Regions { get; set; } = [];
    }
}