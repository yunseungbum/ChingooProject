using Chingoo.Models;

namespace Chingoo.ViewModels
{
    public class HomeViewModel
    {
        public BoardMenuViewModel BoardMenu { get; set; } = new();
        public List<Notice> Notices { get; set; } = new();
        public List<CommunityPost> CommunityPosts { get; set; } = new();
        public List<Post> RecommendedMatches { get; set; } = new();
        public List<Post> RecommendedMercenaries { get; set; } = new();
        public List<Post> RecommendedTeamRecruits { get; set; } = new();
    }
}
