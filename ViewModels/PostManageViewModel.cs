namespace Chingoo.ViewModels
{
    public class PostManageViewModel
    {
        public string Filter { get; set; } = "전체";

        public List<PostManageListItemViewModel> Items { get; set; } = new();
    }

    public class PostManageListItemViewModel
    {
        public int Id { get; set; }

        public string SourceType { get; set; } = string.Empty;

        public string BoardType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? Region { get; set; }

        public string? DayType { get; set; }

        public string? TimeSlot { get; set; }

        public DateTime? MatchDate { get; set; }
    }
}