using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainLeaderboard
    {
        void PostToLeaderboard(float score, string id, out bool success);
        void PostToLeaderboard(float score, string id, string username, out bool success);
        List<LeaderboardScore> LoadLeaderboardScores(string id, TimeScope timeScope, Range range, out bool success);
        LeaderboardScore LoadLeaderboardUserScore(string id, string user, int pageScoreCount, out int page, out bool success);
    }
}