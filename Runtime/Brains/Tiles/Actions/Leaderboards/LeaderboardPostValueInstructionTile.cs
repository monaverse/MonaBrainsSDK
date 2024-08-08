using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Tiles.Actions.Leaderboards.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards
{
    [Serializable]
    public class LeaderboardPostValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "PostToLeaderboard";
        public const string NAME = "Post To Leaderboard";
        public const string CATEGORY = "Leaderboards";
        public override Type TileType => typeof(LeaderboardPostValueInstructionTile);

        [SerializeField] private string _leaderboardName;
        [SerializeField] private string _leaderboardNameName;
        [BrainProperty(true)] public string LeaderboardName { get => _leaderboardName; set => _leaderboardName = value; }
        [BrainPropertyValueName("LeaderboardName", typeof(IMonaVariablesStringValue))] public string LeaderboardNameName { get => _leaderboardNameName; set => _leaderboardNameName = value; }

        [SerializeField] private float _score;
        [SerializeField] private string _scoreName;
        [BrainProperty(true)] public float Score { get => _score; set => _score = value; }
        [BrainPropertyValueName("Score", typeof(IMonaVariablesFloatValue))] public string ScoreName { get => _scoreName; set => _scoreName = value; }

        [SerializeField] private LeaderboardUserType _userType = LeaderboardUserType.ClientUser;
        [BrainPropertyEnum(false)] public LeaderboardUserType UserType { get => _userType; set => _userType = value; }

        [SerializeField] private string _username;
        [SerializeField] private string _usernameName;
        [BrainPropertyShow(nameof(UserType), (int)LeaderboardUserType.DefinedUser)]
        [BrainProperty(false)] public string Username { get => _username; set => _username = value; }
        [BrainPropertyValueName("Username", typeof(IMonaVariablesStringValue))] public string UsernameName { get => _usernameName; set => _usernameName = value; }

        public LeaderboardPostValueInstructionTile() { }

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

            if (!string.IsNullOrEmpty(_leaderboardNameName))
                _leaderboardName = _brain.Variables.GetString(_leaderboardNameName);

            if (!string.IsNullOrEmpty(_scoreName))
                _score = _brain.Variables.GetFloat(_scoreName);

            if (!string.IsNullOrEmpty(_usernameName))
                _username = _brain.Variables.GetString(_usernameName);

            if (_leaderboard == null)
            {
                _leaderboard = _globalBrainRunner.BrainLeaderboards;
                if (_leaderboard == null) return Complete(InstructionTileResult.Success);
            }

            switch (_userType)
            {
                case LeaderboardUserType.ClientUser:
                    _leaderboard.PostToLeaderboard(_score, _leaderboardName, out _);
                    break;
                case LeaderboardUserType.DefinedUser:
                    _leaderboard.PostToLeaderboard(_score, _leaderboardName, _username, out _);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}