using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using Mona.SDK.Core.EasyUI;

namespace Mona.SDK.Brains.Core.Utils.Structs
{
    [System.Serializable]
    public class LeaderboardPage
    {
        public string ID;
        public string Title;
        public int CurrentPage;
        public bool RetrievalSuccess;
        public bool AlwaysShowClient = true;
        public TimeScope Scope;
        public Range BoardRange;
        public int EntriesPerPage = 10;
        public EasyUINumericalBaseFormatType ScoreFormatType = EasyUINumericalBaseFormatType.Default;
        public LeaderboardScore ClientScore;
        public List<LeaderboardScore> Scores = new List<LeaderboardScore>();

        public bool AddClientScoreDisplay => AlwaysShowClient && !string.IsNullOrEmpty(ClientScore.UserName) && !ScoresContainsClient;
        public bool UserPageBefore => AlwaysShowClient && ClientScore.Page < CurrentPage;
        public bool UserPageAfter => AlwaysShowClient && ClientScore.Page > CurrentPage;
        public bool DisplayButtonPageLeft => true;
        public bool DisplayButtonPageRight => true;
        public bool DisplayButtonUserPage => true;

        public int EmptyDisplayCount => EntriesPerPage - Scores.Count;

        private bool ScoresContainsClient
        {
            get
            {
                if (string.IsNullOrEmpty(ClientScore.UserName))
                    return false;

                for (int i = 0; i < Scores.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Scores[i].UserName) && Scores[i].UserName == ClientScore.UserName)
                        return true;
                }

                return false;
            }
        }

        public int EntryCount
        {
            get
            {
                int entryCount = BoardRange.count;

                if (AddClientScoreDisplay)
                    entryCount++;

                return entryCount;
            }
        }
    }
}
