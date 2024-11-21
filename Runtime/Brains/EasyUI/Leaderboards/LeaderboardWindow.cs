using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Mona.SDK.Core.EasyUI;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardWindow : MonoBehaviour
    {
        [SerializeField] private Transform _dataPanel;
        [SerializeField] private GameObject _dataKeyPrefab;
        [SerializeField] private GameObject _dataUserPrefab;
        [SerializeField] private LeaderboardVisualElement _titleText;
        [SerializeField] private LeaderboardVisualElement _pageFirstButton;
        [SerializeField] private LeaderboardVisualElement _pageLeftButton;
        [SerializeField] private LeaderboardVisualElement _pageRightButton;
        [SerializeField] private LeaderboardVisualElement _pagePlayerButton;
        [SerializeField] private string _animCloseTrigger = "Close";
        [SerializeField] private string _fallbackDataKeyName = "Leaderboard_Panel_Data_Key";
        [SerializeField] private string _fallbackDataUserName = "Leaderboard_Panel_Data_Player";
        [SerializeField] private Color[] _colors;
        [SerializeField] private ActivationSettings[] _loadFailureActivations;
        [SerializeField] private UnityEvent _onOpen;
        [SerializeField] private UnityEvent _onClose;

        private TimeScope _scope;
        public TimeScope Scope { get => _scope; set => _scope = value; }

        private Range _pageRange;
        public Range PageRange { get => _pageRange; set => _pageRange = value; }

        private LeaderboardOrderType _scoreOrder;
        public LeaderboardOrderType ScoreOrder { get => _scoreOrder; set => _scoreOrder = value; }

        private IBrainLeaderboardAsync _leaderboardAsync;
        public IBrainLeaderboardAsync LeaderboardAsync { get => _leaderboardAsync; set => _leaderboardAsync = value; }

        private Animator _animator;

        private LeaderboardPage _page;
        
        private List<LeaderboardScoreDisplay> _scoreDisplays = new List<LeaderboardScoreDisplay>();

        private bool CanPageFirst => _page.RetrievalSuccess && _page.CurrentPage > 1;
        private bool CanPageForward => _page.RetrievalSuccess && _page.Scores.Count >= _pageRange.count;
        private bool CanPageBack => _page.RetrievalSuccess && _pageRange.from - _pageRange.count >= 0;
        private bool CanPageHome => _page.RetrievalSuccess && _page.ClientScoreRecorded && !_page.ScoresContainsClient;

        public LeaderboardPage Page
        {
            get { return _page; }
            set
            {
                _page = value;
                UpdatePageVisual();
            }
        }

        [System.Serializable]
        public struct ActivationSettings
        {
            [SerializeField] private bool _setActive;
            [SerializeField] private GameObject _activationObject;

            public bool SetActive => _setActive;
            public GameObject ActivationObject => _activationObject;

            public void SetActivation(bool invert = false)
            {
                if (_activationObject == null)
                    return;

                bool activate = invert ? !_setActive : _setActive;
                _activationObject.SetActive(activate);
            }
        }

        private void Start()
        {
            _onOpen?.Invoke();
        }

        public void InitializeWindow()
        {
            if (_dataKeyPrefab == null)
                _dataKeyPrefab = Resources.Load<GameObject>(_fallbackDataKeyName);

            if (_dataUserPrefab == null)
                _dataUserPrefab = Resources.Load<GameObject>(_fallbackDataUserName);

            gameObject.SetActive(false);
        }

        public void UpdatePageVisual()
        {
            if (_page == null)
                return;

            _scoreDisplays.Clear();

            _titleText.SetText(_page.Title);

            foreach (Transform child in _dataPanel)
                Destroy(child.gameObject);

            ProcessActivationSettings(_loadFailureActivations, true);

            if (!_page.RetrievalSuccess)
            {
                ProcessActivationSettings(_loadFailureActivations, false);
                return;
            }

            int entryCount = _page.EntryCount;
            Instantiate(_dataKeyPrefab, _dataPanel);

            int entryIndex = 0;

            if (_page.UserPageBefore)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.EntryIndex = entryIndex;
                scoreDisplay.SetScore(_page.ClientScore, _page.ScoreFormatType, _page.RoundScore);
                scoreDisplay.PlayBounceAnimation();
                entryIndex++;
            }

            for (int i = 0; i < _page.Scores.Count; i++)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.EntryIndex = entryIndex;
                scoreDisplay.SetScore(_page.Scores[i], _page.ScoreFormatType, _page.RoundScore);

                if (_page.ScoresContainsClient && _page.ClientScore.UserName == _page.Scores[i].UserName)
                    scoreDisplay.PlayBounceAnimation();

                entryIndex++;
            }

            for (int i = 0; i < _page.EmptyDisplayCount; i++)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.EntryIndex = entryIndex;
                scoreDisplay.SetEmpty();

                entryIndex++;
            }

            if (_page.UserPageAfter)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.EntryIndex = entryIndex;
                scoreDisplay.SetScore(_page.ClientScore, _page.ScoreFormatType, _page.RoundScore);
                scoreDisplay.PlayBounceAnimation();
                entryIndex++;
            }

            SetButtonDisplays();
        }

        private void SetButtonDisplays()
        {
            if (_pageFirstButton != null)
                _pageFirstButton.ToggleActivation(CanPageFirst);

            if (_pageLeftButton != null)
                _pageLeftButton.ToggleActivation(CanPageBack);

            if (_pageRightButton != null)
                _pageRightButton.ToggleActivation(CanPageForward);

            if (_pagePlayerButton != null)
                _pagePlayerButton.ToggleActivation(CanPageHome);
        }

        public async void PageFirst()
        {
            if (_leaderboardAsync == null || !CanPageFirst)
                return;

            _pageRange = new Range(0, _pageRange.count);
            await ProcessPageData();
        }

        public async void PageForward()
        {
            if (_leaderboardAsync == null || !CanPageForward)
                return;

            _pageRange = new Range(_pageRange.from + _pageRange.count, _pageRange.count);
            await ProcessPageData();

        }

        public async void PageBack()
        {
            if (_leaderboardAsync == null || !CanPageBack)
                return;

            _pageRange = new Range(_pageRange.from - _pageRange.count, _pageRange.count);
            await ProcessPageData();
        }

        public async void PageClientHome()
        {
            if (_leaderboardAsync == null || !CanPageHome)
                return;

            int basePage = _page.ClientScore.Rank / _pageRange.count;
            int pageStart = basePage * _pageRange.count;
            _pageRange = new Range(pageStart, _pageRange.count);

            await ProcessPageData();
        }

        public async Task ProcessPageData()
        {
            BrainProcess brainProcess = await _leaderboardAsync.LoadScores(_page.ID, _scope, _pageRange, _scoreOrder);

            if (!brainProcess.WasSuccessful)
                return;

            _page.Scores = brainProcess.GetScores();
            _page.CurrentPage = _pageRange.from / _pageRange.count;
            UpdatePageVisual();
        }

        public LeaderboardScoreDisplay CreateScoreDisplay()
        {
            GameObject go = Instantiate(_dataUserPrefab, _dataPanel);
            LeaderboardScoreDisplay scoreDisplay = go.GetComponent<LeaderboardScoreDisplay>();
            _scoreDisplays.Add(scoreDisplay);

            return scoreDisplay;
        }

        public Color GetColor(int index)
        {
            if (_colors.Length > 0 && index > 0 && index < _colors.Length)
                return _colors[index];

            return Color.magenta;
        }

        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void CloseWindow()
        {
            if (_onClose != null) _onClose?.Invoke();
            else if (_animator) _animator.SetTrigger(_animCloseTrigger);
            else Deactivate();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void ProcessActivationSettings(ActivationSettings[] settings, bool invertActivation = false)
        {
            if (settings == null)
                return;

            for (int i = 0; i < settings.Length; i++)
            {
                settings[i].SetActivation(invertActivation);
            }
        }
    }
}
