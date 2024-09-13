using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Mona.SDK.Core.EasyUI;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardWindow : MonoBehaviour
    {
        [SerializeField] private Transform _dataPanel;
        [SerializeField] private GameObject _dataKeyPrefab;
        [SerializeField] private GameObject _dataUserPrefab;
        [SerializeField] private LeaderboardVisualElement _titleText;
        [SerializeField] private LeaderboardVisualElement _pageLeftButton;
        [SerializeField] private LeaderboardVisualElement _pageRightButton;
        [SerializeField] private LeaderboardVisualElement _pagePlayerButton;
        [SerializeField] private string _animCloseTrigger = "Close";
        [SerializeField] private string _fallbackDataKeyName = "Leaderboard_Panel_Data_Key";
        [SerializeField] private string _fallbackDataUserName = "Leaderboard_Panel_Data_Player";
        [SerializeField] private Color[] _colors;
        [SerializeField] private UnityEvent _onOpen;
        [SerializeField] private UnityEvent _onClose;

        private Animator _animator;
        private LeaderboardPage _page;
        private List<LeaderboardScoreDisplay> _scoreDisplays = new List<LeaderboardScoreDisplay>();

        public LeaderboardPage Page
        {
            get { return _page; }
            set
            {
                _page = value;
                UpdatePageVisual();
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

            int entryCount = _page.EntryCount;
            Instantiate(_dataKeyPrefab, _dataPanel);

            if (_page.UserPageBefore)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.SetScore(_page.ClientScore, _page.ScoreFormatType);
            }

            for (int i = 0; i < _page.Scores.Count; i++)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.SetScore(_page.Scores[i], _page.ScoreFormatType);
            }

            for (int i = 0; i < _page.EmptyDisplayCount; i++)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.SetEmpty();
            }

            if (_page.UserPageAfter)
            {
                LeaderboardScoreDisplay scoreDisplay = CreateScoreDisplay();
                scoreDisplay.SetScore(_page.ClientScore, _page.ScoreFormatType);
            }
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
    }
}
