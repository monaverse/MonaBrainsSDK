
namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainSocialPlatformUser
    {
        void RegisterNewUser(string username, string password, out bool success);
        void LoginUser(string username, out bool success);
        void LoginUser(string username, string password, out bool success);
        void LogoutCurrentUser(out bool success);
        void ChangeCurrentUserPassword(string newPassword, out bool success);
    }
}