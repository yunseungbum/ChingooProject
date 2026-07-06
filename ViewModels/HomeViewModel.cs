using Chingoo.Models;

namespace Chingoo.ViewModels
{
    public class HomeViewModel
    {
        public BoardMenuViewModel BoardMenu { get; set; } = new();
        public List<Notice> Notices { get; set; } = new();
    }
}
