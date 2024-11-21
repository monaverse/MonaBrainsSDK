using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Core.EasyUI;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;

using TMPro;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] _rankTexts;
        [SerializeField] private TMP_Text[] _usernameTexts;
        [SerializeField] private TMP_Text[] _scoreTexts;
        [SerializeField] private float _initialDelay = 0.2f;
        [SerializeField] private float _revealTime = 0.15f;
        [SerializeField] private string _animRevealTrigger = "Reveal";
        [SerializeField] private string _animBounceTrigger = "Bounce";
        [SerializeField] private string _emptyText = "- - -";
        [SerializeField] private AudioClassificationType _audioType = AudioClassificationType.SoundEffect;
        [SerializeField] private AudioClip _revealClip;
        [SerializeField] private float _clipVolume = 1f;

        private Animator _animator;
        private AudioSource _audioSource;
        private MonaBrainAudio _brainAudio;

        [HideInInspector] public int EntryIndex;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _brainAudio = MonaBrainAudio.Instance;
        }

        private void Start()
        {
            if (_animator != null)
                StartCoroutine(RevealEntry());
        }

        private IEnumerator RevealEntry()
        {
            yield return new WaitForSeconds(_initialDelay);
            float revealDelay = EntryIndex * _revealTime;
            yield return new WaitForSeconds(revealDelay);
            _animator.SetTrigger(_animRevealTrigger);

            if (_brainAudio != null && _audioSource != null && _revealClip != null)
            {
                float volume = _brainAudio.GetVolumeLevel(_audioType) * _clipVolume;
                _audioSource.volume = volume;
                _audioSource.clip = _revealClip;
                _audioSource.Play();
            }
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

        public void SetScore(LeaderboardScore score, EasyUINumericalBaseFormatType formatType, bool roundScore)
        {
            for (int i = 0; i < _rankTexts.Length; i++)
                _rankTexts[i].text = score.Rank.ToString();

            for (int i = 0; i < _usernameTexts.Length; i++)
                _usernameTexts[i].text = score.UserName;

            for (int i = 0; i < _scoreTexts.Length; i++)
                _scoreTexts[i].text = FormatScore(float.Parse(score.Score), formatType, roundScore);
        }

        public void PlayBounceAnimation()
        {
            if (!_animator)
                _animator = GetComponent<Animator>();

            if (_animator)
                _animator.SetTrigger(_animBounceTrigger);
        }

        public string FormatScore(float value, EasyUINumericalBaseFormatType formatType, bool roundScore)
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
                    if(roundScore)
                        return (value * 100f).ToString("0") + "%";
                    else
                        return (value * 100f).ToString("N", currentCulture) + "%";

                default:
                    if (roundScore)
                        return value.ToString("0");
                    else
                        return value.ToString("N", currentCulture);
            }
        }
    }
}
