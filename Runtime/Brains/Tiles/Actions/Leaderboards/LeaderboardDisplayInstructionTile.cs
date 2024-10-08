using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Brains.Tiles.Actions.Leaderboards.Enums;
using Mona.SDK.Core.EasyUI;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;
using UnityEngine.SocialPlatforms;
using Mona.SDK.Brains.EasyUI.Leaderboards;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards
{
    [Serializable]
    public class LeaderboardDisplayInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "DisplayLeaderboard";
        public const string NAME = "Display Leaderboard";
        public const string CATEGORY = "Leaderboards";
        public override Type TileType => typeof(LeaderboardDisplayInstructionTile);

        [SerializeField] private string _leaderboardName;
        [SerializeField] private string _leaderboardNameName;
        [BrainProperty(true)] public string LeaderboardName { get => _leaderboardName; set => _leaderboardName = value; }
        [BrainPropertyValueName("LeaderboardName", typeof(IMonaVariablesStringValue))] public string LeaderboardNameName { get => _leaderboardNameName; set => _leaderboardNameName = value; }

        [SerializeField] private PageDisplay _pageToLoad = PageDisplay.ClientPage;
        [BrainPropertyEnum(true)] public PageDisplay PageToLoad { get => _pageToLoad; set => _pageToLoad = value; }

        [SerializeField] private string _username;
        [SerializeField] private string _usernameName;
        [BrainPropertyShow(nameof(PageToLoad), (int)PageDisplay.UserPage)]
        [BrainProperty(true)] public string Username { get => _username; set => _username = value; }
        [BrainPropertyValueName("Username", typeof(IMonaVariablesStringValue))] public string UsernameName { get => _usernameName; set => _usernameName = value; }

        [SerializeField] private float _pageIndex = 0f;
        [SerializeField] private string _pageIndexName;
        [BrainPropertyShow(nameof(PageToLoad), (int)PageDisplay.PageIndex)]
        [BrainProperty(true)] public float PageIndex { get => _pageIndex; set => _pageIndex = value; }
        [BrainPropertyValueName("PageIndex", typeof(IMonaVariablesFloatValue))] public string PageIndexName { get => _pageIndexName; set => _pageIndexName = value; }

        [SerializeField] private EasyUINumericalBaseFormatType _scoreFormat = EasyUINumericalBaseFormatType.Default;
        [BrainPropertyEnum(true)] public EasyUINumericalBaseFormatType ScoreFormat { get => _scoreFormat; set => _scoreFormat = value; }

        [SerializeField] private TimeScope _scope = TimeScope.AllTime;
        [BrainPropertyEnum(false)] public TimeScope Scope { get => _scope; set => _scope = value; }

        [SerializeField] private float _scoresPerPage = 10f;
        [SerializeField] private string _scoresPerPageName;
        [BrainProperty(false)] public float ScoresPerPage { get => _scoresPerPage; set => _scoresPerPage = value; }
        [BrainPropertyValueName("ScoresPerPage", typeof(IMonaVariablesFloatValue))] public string ScoresPerPageName { get => _scoresPerPageName; set => _scoresPerPageName = value; }

        [SerializeField] private LeaderboardOrderType _scoreOrder = LeaderboardOrderType.Default;
        [BrainPropertyEnum(false)] public LeaderboardOrderType ScoreOrder { get => _scoreOrder; set => _scoreOrder = value; }

        [SerializeField] private bool _alwaysShowClient = true;
        [SerializeField] private string _alwaysShowClientName;
        [BrainProperty(false)] public bool AlwaysShowClient { get => _alwaysShowClient; set => _alwaysShowClient = value; }
        [BrainPropertyValueName("AlwaysShowClient", typeof(IMonaVariablesBoolValue))] public string AlwaysShowClientName { get => _alwaysShowClientName; set => _alwaysShowClientName = value; }

        [SerializeField] private bool _useAlternateTitle = false;
        [SerializeField] private string _useAlternateTitleName;
        [BrainProperty(false)] public bool UseAlternateTitle { get => _useAlternateTitle; set => _useAlternateTitle = value; }
        [BrainPropertyValueName("UseAlternateTitle", typeof(IMonaVariablesBoolValue))] public string UseAlternateTitleName { get => _useAlternateTitleName; set => _useAlternateTitleName = value; }

        [SerializeField] private string _alternateTitle;
        [SerializeField] private string _alternateTitleName;
        [BrainPropertyShow(nameof(DisplayAlternateName), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public string AlternateTitle { get => _alternateTitle; set => _alternateTitle = value; }
        [BrainPropertyValueName("AlternateTitle", typeof(IMonaVariablesStringValue))] public string AlternateTitleName { get => _alternateTitleName; set => _alternateTitleName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        private int _truePageIndex;
        private bool _active;
        private bool _isRunning;
        private readonly string _failedLeaderboardTitle = "Leaderboard Error";

        private BoardProcessingState _pageProcessingState = BoardProcessingState.NotProcessing;
        private BoardProcessingState _userProcessingState = BoardProcessingState.NotProcessing;
        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private LeaderboardDisplayController _leaderboardDisplay;
        private IBrainLeaderboardAsync _leaderboardServer;
        private BrainProcess _pageProcess;
        private BrainProcess _userProcess;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private LeaderboardScore _clientScore;
        private List<LeaderboardScore> _scores = new List<LeaderboardScore>();
        private LeaderboardWindow _window;
        private UnityEngine.SocialPlatforms.Range _pageRange;

        private bool PageLoadRequiresUserData => _alwaysShowClient || _pageToLoad == PageDisplay.ClientPage || _pageToLoad == PageDisplay.UserPage;
        public UIDisplayType DisplayAlternateName => _useAlternateTitle ? UIDisplayType.Show : UIDisplayType.Hide;

        private bool UserIsProcessing => _userProcessingState == BoardProcessingState.CanProcess || _userProcessingState == BoardProcessingState.ProcessStarted || (_userProcess != null && _userProcess.IsProcessing);
        private bool PageIsProcessing => _pageProcessingState == BoardProcessingState.CanProcess || _pageProcessingState == BoardProcessingState.ProcessStarted || (_pageProcess != null && _pageProcess.IsProcessing);
        private bool AnyProcessing => UserIsProcessing || PageIsProcessing;

        public enum UIDisplayType
        {
            Show = 0,
            Hide = 10
        }

        [Serializable]
        public enum PageDisplay
        {
            ClientPage = 0,
            UserPage = 10,
            FirstPage = 20,
            PageIndex = 30
        }

        public enum BoardProcessingState
        {
            NotProcessing = 0,
            CanProcess = 10,
            ProcessStarted = 20,
            ProcessComplete = 30
        }

        public LeaderboardDisplayInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            _leaderboardDisplay = LeaderboardDisplayController.Instance;
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
            if (PageLoadRequiresUserData)
            {
                if (UserIsProcessing)
                    return;

                if (_pageProcess == null && _pageProcessingState == BoardProcessingState.NotProcessing)
                {
                    _clientScore = _userProcess.GetUserScore();
                    _truePageIndex = 0;

                    switch (_pageToLoad)
                    {
                        case PageDisplay.ClientPage:
                        case PageDisplay.UserPage:
                            _truePageIndex = _clientScore.Page;
                            break;
                        case PageDisplay.PageIndex:
                            _truePageIndex = (int)_pageIndex;
                            break;
                    }

                    _pageRange = new UnityEngine.SocialPlatforms.Range(_truePageIndex, (int)_scoresPerPage);
                    ProcessPageData();
                }
            }

            if (_userProcessingState != BoardProcessingState.NotProcessing && _pageProcessingState == BoardProcessingState.NotProcessing)
            {
                StartStandardPageProcess();
                return;
            }

            if (PageIsProcessing)
            {
                return;
            }

            // *************************************************
            // PAGE AND (OPTIONAL) USER DATA PROCESSING COMPLETE
            // DISPLAY LEADERBOARD WINDOW
            // *************************************************

            if (_pageProcess.WasSuccessful)
            {
                string title = _useAlternateTitle ? _alternateTitle : _pageProcess.GetString();

                LeaderboardPage page = new LeaderboardPage
                {
                    ID = _leaderboardName,
                    Title = title,
                    CurrentPage = _truePageIndex,
                    RetrievalSuccess = true,
                    AlwaysShowClient = _alwaysShowClient,
                    BoardRange = _pageRange,
                    EntriesPerPage = (int)_scoresPerPage,
                    ScoreFormatType = _scoreFormat,
                    Scores = _pageProcess.GetScores()
                };

                if (_userProcess != null)
                    page.ClientScore = _userProcess.GetUserScore();

                _window.Display();
                _window.Page = page;
            }
            else
            {
                string title = _useAlternateTitle ? _alternateTitle : _failedLeaderboardTitle;

                LeaderboardPage page = new LeaderboardPage
                {
                    ID = _leaderboardName,
                    Title = title,
                    RetrievalSuccess = false
                };

                _window.Display();
                _window.Page = page;
            }

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, _pageProcess.WasSuccessful);

            _pageProcessingState = BoardProcessingState.NotProcessing;
            _userProcessingState = BoardProcessingState.NotProcessing;
            _isRunning = false;

            Complete(InstructionTileResult.Success, true);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null || _leaderboardDisplay == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (_window == null)
                _window = _leaderboardDisplay.Window;

            if (!_isRunning)
            {
                if (!string.IsNullOrEmpty(_leaderboardNameName))
                    _leaderboardName = _brain.Variables.GetString(_leaderboardNameName);

                if (!string.IsNullOrEmpty(_usernameName))
                    _username = _brain.Variables.GetString(_usernameName);

                if (!string.IsNullOrEmpty(_pageIndexName))
                    _pageIndex = _brain.Variables.GetFloat(_pageIndexName);

                if (!string.IsNullOrEmpty(_scoresPerPageName))
                    _scoresPerPage = _brain.Variables.GetFloat(_scoresPerPageName);

                if (!string.IsNullOrEmpty(_useAlternateTitleName))
                    _useAlternateTitle = _brain.Variables.GetBool(_useAlternateTitleName);

                if (!string.IsNullOrEmpty(_alternateTitleName))
                    _alternateTitle = _brain.Variables.GetString(_alternateTitleName);

                if (_leaderboardServer == null)
                {
                    _leaderboardServer = _globalBrainRunner.BrainLeaderboards;

                    if (_leaderboardServer == null)
                        return Complete(InstructionTileResult.Success);
                }

                _userProcessingState = BoardProcessingState.NotProcessing;
                _pageProcessingState = BoardProcessingState.NotProcessing;
                _pageProcess = null;
                _userProcess = null;

                if (PageLoadRequiresUserData)
                {
                    ProcessUserData();
                }
                else
                {
                    StartStandardPageProcess();
                }

                if ((_pageProcessingState != BoardProcessingState.NotProcessing && _pageProcessingState != BoardProcessingState.ProcessComplete) || _userProcessingState != BoardProcessingState.NotProcessing)
                    AddFixedTickDelegate();
            }

            if (AnyProcessing)
                return Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async Task ProcessUserData()
        {
            _userProcessingState = BoardProcessingState.CanProcess;
            _userProcessingState = BoardProcessingState.ProcessStarted;
            _userProcess = _pageToLoad == PageDisplay.UserPage ?
                        await _leaderboardServer.LoadUserScore(_leaderboardName, _username, (int)_scoresPerPage) :
                        await _leaderboardServer.LoadClientScore(_leaderboardName, (int)_scoresPerPage);

            _userProcessingState = BoardProcessingState.ProcessComplete;
        }

        private async Task StartStandardPageProcess()
        {
            _pageProcessingState = BoardProcessingState.CanProcess;
            _truePageIndex = _pageToLoad == PageDisplay.PageIndex ? (int)_pageIndex : 0;
            _pageRange = new UnityEngine.SocialPlatforms.Range(_truePageIndex, (int)_scoresPerPage);
            await ProcessPageData();
        }

        private async Task ProcessPageData()
        {
            _pageProcessingState = BoardProcessingState.ProcessStarted;
            _pageProcess = await _leaderboardServer.LoadScores(_leaderboardName, _scope, _pageRange, _scoreOrder);
            _pageProcessingState = BoardProcessingState.ProcessComplete;
        }
    }
}