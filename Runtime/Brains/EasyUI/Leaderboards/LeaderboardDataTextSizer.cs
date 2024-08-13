using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardDataTextSizer : MonoBehaviour
    {
        [SerializeField] private List<TMP_Text> _invisibleText = new List<TMP_Text>();
        [SerializeField] private List<TMP_Text> _visibleText = new List<TMP_Text>();

        private const float _minTextSize = 8f;
        private float _currentTextSize = -1f;
        private float _previousTextSize = 8f;

        private float SmallestTextSize
        {
            get
            {
                float smallestSize = _minTextSize;

                for (int i = 0; i < _invisibleText.Count; i++)
                {
                    if (Mathf.Approximately(smallestSize, _minTextSize) || (_invisibleText[i].fontSize < smallestSize && _invisibleText[i].fontSize >= _minTextSize))
                        smallestSize = _invisibleText[i].fontSize;
                }

                return smallestSize;
            }
        }

        private void Update()
        {
            _currentTextSize = SmallestTextSize;

            if (_previousTextSize != _currentTextSize)
            {
                SetVisibleTextSize();
                _previousTextSize = _currentTextSize;
            }
        }

        private void SetVisibleTextSize()
        {
            for (int i = 0; i < _visibleText.Count; i++)
            {
                _visibleText[i].enableAutoSizing = false;
                _visibleText[i].fontSize = _currentTextSize;
            }
        }
    }
}


