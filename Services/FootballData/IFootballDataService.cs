using Chingoo.ViewModels.FootballData;

namespace Chingoo.Services.FootballData
{
    public interface IFootballDataService
    {
        Task<WorldCupMatchesViewModel> GetWorldCupMatchesAsync(int season = 2026, CancellationToken cancellationToken = default);
    }
}
