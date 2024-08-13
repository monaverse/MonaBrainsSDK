using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Brains.Core.Utils.Structs;
using TMPro;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _rankTexts;
        [SerializeField] private TMP_Text[] _usernameTexts;
        [SerializeField] private TMP_Text[] _scoreTexts;
        [SerializeField] private string _animBounceTrigger = "Bounce";
        [SerializeField] private string _emptyText = "- - -";

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetEmpty()
        {
            for (int i = 0; i < _rankTexts.Length; i++)
                _rankTexts[i].text = _emptyText;

            for (int i = 0; i < _usernameTexts.Length; i++)
                _usernameTexts[i].text = _emptyText;

            for (int i = 0; i < _scoreTexts.Length; i++)
                _scoreTexts[i].text = _emptyText;
        }

        public void SetScore(LeaderboardScore score)
        {
            for (int i = 0; i < _rankTexts.Length; i++)
                _rankTexts[i].text = score.Rank.ToString();

            for (int i = 0; i < _usernameTexts.Length; i++)
                _usernameTexts[i].text = score.UserName;

            for (int i = 0; i < _scoreTexts.Length; i++)
                _scoreTexts[i].text = score.Score;
        }

        public void PlayBounceAnimation()
        {
            if (_animator)
                _animator.SetTrigger(_animBounceTrigger);
        }
    }
}
