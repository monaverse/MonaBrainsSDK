using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardVisualElement : MonoBehaviour
    {
        [SerializeField] private bool _displayElement;
        [SerializeField] private bool _colorWithID;
        [SerializeField] private int _colorID;

        private Image _image;
        private TMP_Text _text;
        private LeaderboardWindow _window;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            _window = GetComponentInParent<LeaderboardWindow>();
            UpdateColor();
        }

        public void SetText(string newText)
        {
            if (_text != null)
                _text.text = newText;
        }

        public void UpdateColor()
        {
            if (_window == null || !_colorWithID)
                return;

            if (_image != null)
                _image.color = _window.GetColor(_colorID);

            if (_text != null)
                _text.color = _window.GetColor(_colorID);
        }

        public void UpdateColor(Color newColor)
        {
            if (_image != null)
                _image.color = newColor;

            if (_text != null)
                _text.color = newColor;
        }

        public void UpdateFont(TMP_FontAsset font)
        {
            if (_text != null)
                _text.font = font;
        }

        public void ToggleDisplay(bool display)
        {
            if (_image != null)
                _image.enabled = display;

            if (_text != null)
                _text.enabled = display;
        }

        public void ToggleActivation(bool isActive)
        {
            if (_image != null)
                _image.gameObject.SetActive(isActive);

            if (_text != null)
                _text.gameObject.SetActive(isActive);
        }
    }
}
