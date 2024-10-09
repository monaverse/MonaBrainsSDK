using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IMonaLeaderboardAsync : IBrainLeaderboardAsync
    {
        Task<BrainProcess> AutoLogin(string environment, string applicationId, string secret);
    }

    public interface IBrainLeaderboardAsync : IBrainSocialPlatformUserAsync
    {
        bool LeaderboardEnabled { get; }

        Task<BrainProcess> PostToLeaderboard(float score, string id);
        Task<BrainProcess> PostToLeaderboard(float score, string id, string username);
        Task<BrainProcess> LoadScores(string id, TimeScope timeScope, Range range, LeaderboardOrderType order = LeaderboardOrderType.Default);
        Task<BrainProcess> LoadClientScore(string id, int pageScoreCount, string order = null);
        Task<BrainProcess> LoadUserScore(string id, string user, int pageScoreCount);
    }
}