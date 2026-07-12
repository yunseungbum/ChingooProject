using System.Security.Claims;
using Chingoo.ViewModels;

namespace Chingoo.Services.Home
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewModelAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
    }
}
