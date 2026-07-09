using System.Security.Claims;
using Chingoo.ViewModels;

namespace Chingoo.Services.Home
{
    public interface IHomeService
    {
        HomeViewModel GetHomeViewModel(ClaimsPrincipal user);
    }
}
