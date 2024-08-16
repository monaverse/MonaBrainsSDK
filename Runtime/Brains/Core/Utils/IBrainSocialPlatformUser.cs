
namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainSocialPlatformUser
    {
        BrainProcess RegisterNewUser(string username, string password);
        BrainProcess AutoLogin();
        BrainProcess LoginUser(string username);
        BrainProcess LoginUser(string username, string password);
        BrainProcess LogoutCurrentUser();
        BrainProcess ChangeCurrentUserPassword(string newPassword);
    }
}