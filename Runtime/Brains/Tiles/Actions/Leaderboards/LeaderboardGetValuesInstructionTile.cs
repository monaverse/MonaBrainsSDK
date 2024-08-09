using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Brains.Tiles.Actions.Leaderboards.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards
{
    [Serializable]
    public class LeaderboardGetValuesInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "GetLeaderboardValues";
        public const string NAME = "Get Leaderboard Values";
        public const string CATEGORY = "Leaderboards";
        public override Type TileType => typeof(LeaderboardGetValuesInstructionTile);

        [SerializeField] private string _leaderboardName;
        [SerializeField] private string _leaderboardNameName;
        [BrainProperty(true)] public string LeaderboardName { get => _leaderboardName; set => _leaderboardName = value; }
        [BrainPropertyValueName("LeaderboardName", typeof(IMonaVariablesStringValue))] public string LeaderboardNameName { get => _leaderboardNameName; set => _leaderboardNameName = value; }

        [SerializeField] private string _rank;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string Rank { get => _rank; set => _rank = value; }

        [SerializeField] private string _score;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string Score { get => _score; set => _score = value; }

        [SerializeField] private string _page;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string Page { get => _page; set => _page = value; }

        [SerializeField] private LeaderboardUserType _userType = LeaderboardUserType.ClientUser;
        [BrainPropertyEnum(false)] public LeaderboardUserType UserType { get => _userType; set => _userType = value; }

        [SerializeField] private string _username;
        [SerializeField] private string _usernameName;
        [BrainPropertyShow(nameof(UserType), (int)LeaderboardUserType.DefinedUser)]
        [BrainProperty(false)] public string Username { get => _username; set => _username = value; }
        [BrainPropertyValueName("Username", typeof(IMonaVariablesStringValue))] public string UsernameName { get => _usernameName; set => _usernameName = value; }

        [SerializeField] private float _scoresPerPage = 10f;
        [SerializeField] private string _scoresPerPageName;
        [BrainProperty(true)] public float ScoresPerPage { get => _scoresPerPage; set => _scoresPerPage = value; }
        [BrainPropertyValueName("ScoresPerPage", typeof(IMonaVariablesFloatValue))] public string ScoresPerPageName { get => _scoresPerPageName; set => _scoresPerPageName = value; }

        public LeaderboardGetValuesInstructionTile() { }

        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainLeaderboard _leaderboard;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (string.IsNullOrEmpty(_rank) && string.IsNullOrEmpty(_score) && string.IsNullOrEmpty(_page))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_usernameName))
                _username = _brain.Variables.GetString(_usernameName);

            if (!string.IsNullOrEmpty(_scoresPerPageName))
                _scoresPerPage = _brain.Variables.GetFloat(_scoresPerPageName);

            if (_leaderboard == null)
            {
                _leaderboard = _globalBrainRunner.BrainLeaderboards;
                if (_leaderboard == null) return Complete(InstructionTileResult.Success);
            }

            LeaderboardScore score = new LeaderboardScore();
            bool success = false;

            switch (_userType)
            {
                case LeaderboardUserType.ClientUser:
                    score = _leaderboard.LoadLeaderboardClientScore(_leaderboardName, (int)_scoresPerPage, out success);
                    break;
                case LeaderboardUserType.DefinedUser:
                    score = _leaderboard.LoadLeaderboardUserScore(_leaderboardName, _username, (int)_scoresPerPage, out success);
                    break;
            }

            if (!success)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_rank))
                _brain.Variables.Set(_rank, (float)score.Rank);

            if (!string.IsNullOrEmpty(_score))
                _brain.Variables.Set(_score, float.Parse(score.Score));

            if (!string.IsNullOrEmpty(_page))
                _brain.Variables.Set(_page, (float)score.Page);

            return Complete(InstructionTileResult.Success);
        }
    }
}