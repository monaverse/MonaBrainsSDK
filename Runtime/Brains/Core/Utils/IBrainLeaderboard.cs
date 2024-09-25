using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainLeaderboard : IBrainSocialPlatformUserAsync
    {
        bool LeaderboardEnabled { get; }

        BrainProcess PostToLeaderboard(float score, string id);
        BrainProcess PostToLeaderboard(float score, string id, string username);
        BrainProcess LoadScores(string id, TimeScope timeScope, Range range);
        BrainProcess LoadClientScore(string id, int pageScoreCount);
        BrainProcess LoadUserScore(string id, string user, int pageScoreCount);
    }
}