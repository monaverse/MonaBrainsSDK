using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Brains.Tiles.Actions.Leaderboards.Enums;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards
{
    [Serializable]
    public class LeaderboardGetValuesInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
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
        [BrainProperty(false)] public float ScoresPerPage { get => _scoresPerPage; set => _scoresPerPage = value; }
        [BrainPropertyValueName("ScoresPerPage", typeof(IMonaVariablesFloatValue))] public string ScoresPerPageName { get => _scoresPerPageName; set => _scoresPerPageName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        private bool _active;
        private bool _isRunning;
        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainLeaderboard _leaderboard;
        private BrainProcess _serverProcess;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        public LeaderboardGetValuesInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_isRunning)
                    LostControl();

                return;
            }

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }

        public override void Unload(bool destroy = false)
        {
            SetActive(false);
            _isRunning = false;
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _isRunning;
        }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick();
        }

        private void FixedTick()
        {
            if (_leaderboard == null || _serverProcess == null || _serverProcess.IsProcessing)
                return;

            if (_serverProcess.WasSuccessful)
            {
                LeaderboardScore score = _serverProcess.GetUserScore();

                if (!string.IsNullOrEmpty(_rank))
                    _brain.Variables.Set(_rank, (float)score.Rank);

                if (!string.IsNullOrEmpty(_score))
                    _brain.Variables.Set(_score, float.Parse(score.Score));

                if (!string.IsNullOrEmpty(_page))
                    _brain.Variables.Set(_page, (float)score.Page);

            }

            _isRunning = false;

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, _serverProcess.WasSuccessful);

            Complete(InstructionTileResult.Success, true);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!_isRunning)
            {
                if (!string.IsNullOrEmpty(_leaderboardNameName))
                    _leaderboardName = _brain.Variables.GetString(_leaderboardNameName);

                if (!string.IsNullOrEmpty(_usernameName))
                    _username = _brain.Variables.GetString(_usernameName);

                if (!string.IsNullOrEmpty(_scoresPerPageName))
                    _scoresPerPage = _brain.Variables.GetFloat(_scoresPerPageName);

                if (_leaderboard == null)
                {
                    _leaderboard = _globalBrainRunner.BrainLeaderboards;

                    if (_leaderboard == null)
                        return Complete(InstructionTileResult.Success);
                }

                switch (_userType)
                {
                    case LeaderboardUserType.ClientUser:
                        _serverProcess = _leaderboard.LoadClientScore(_leaderboardName, (int)_scoresPerPage);
                        break;
                    case LeaderboardUserType.DefinedUser:
                        _serverProcess = _leaderboard.LoadUserScore(_leaderboardName, _username, (int)_scoresPerPage);
                        break;
                }

                AddFixedTickDelegate();
            }

            if (_serverProcess != null)
                return Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}