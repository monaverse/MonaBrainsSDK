
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainSocialPlatformUserAsync
    {
        Task<BrainProcess> RegisterNewUser(string username, string password);
        Task<BrainProcess> AutoLogin();
        Task<BrainProcess> LoginUser(string username);
        Task<BrainProcess> LoginUser(string username, string password);
        Task<BrainProcess> LogoutCurrentUser();
        Task<BrainProcess> ChangeCurrentUserPassword(string newPassword);
    }
}