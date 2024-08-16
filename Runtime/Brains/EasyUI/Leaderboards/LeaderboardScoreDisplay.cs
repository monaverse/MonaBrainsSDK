using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Core.EasyUI;
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

        public void SetScore(LeaderboardScore score, EasyUINumericalBaseFormatType formatType)
        {
            for (int i = 0; i < _rankTexts.Length; i++)
                _rankTexts[i].text = score.Rank.ToString();

            for (int i = 0; i < _usernameTexts.Length; i++)
                _usernameTexts[i].text = score.UserName;

            for (int i = 0; i < _scoreTexts.Length; i++)
                _scoreTexts[i].text = FormatScore(float.Parse(score.Score), formatType);
        }

        public void PlayBounceAnimation()
        {
            if (_animator)
                _animator.SetTrigger(_animBounceTrigger);
        }

        public string FormatScore(float value, EasyUINumericalBaseFormatType formatType)
        {
            // Get the current culture info
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            switch (formatType)
            {
                case EasyUINumericalBaseFormatType.Time:
                    TimeSpan timeSpan = TimeSpan.FromSeconds(value);
                    return string.Format(currentCulture, "{0:D2}:{1:D2}:{2:D2}.{3:D2}",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds / 10);

                case EasyUINumericalBaseFormatType.Currency:
                    return value.ToString("C", currentCulture);

                case EasyUINumericalBaseFormatType.Percentage:
                    return (value * 100f).ToString("N", currentCulture) + "%";

                default:
                    return value.ToString("N", currentCulture);
            }
        }
    }
}
