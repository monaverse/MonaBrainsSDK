using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;
using Mona.SDK.Brains.EasyUI.ScreenInput.Structs;

namespace Mona.SDK.Brains.EasyUI.ScreenInput
{
    public class EasyUIScreenInput : MonoBehaviour
    {
        private static EasyUIScreenInput _instance;
        public static EasyUIScreenInput Instance => _instance;

        [SerializeField] private string _fallbackScreenInputName = "Prefab_EasyUIScreenInputCanvas";
        [SerializeField] private GameObject _inputCanvas;
        [SerializeField] private EasyUIScreenJoypad _joypadLeft;
        [SerializeField] private EasyUIScreenJoypad _joypadRight;
        [SerializeField] private EventSystem _eventSystem;

        private bool _setupComplete;
        private StandaloneInputModule _baseInputModule;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(this);
        }

        public void Initialize()
        {
            if (_setupComplete)
                return;

            if (_eventSystem == null)
            {
                _eventSystem = FindFirstObjectByType<EventSystem>();

                if (_eventSystem == null)
                {
                    _eventSystem = gameObject.AddComponent<EventSystem>();
                }
            }

            if (_baseInputModule == null)
            {
                _baseInputModule = FindFirstObjectByType<StandaloneInputModule>();

                if (_baseInputModule == null)
                {
                    _baseInputModule = gameObject.AddComponent<StandaloneInputModule>();
                }
            }

            if (_inputCanvas == null)
            {
                GameObject canvasPrefab = Resources.Load<GameObject>(_fallbackScreenInputName);
                _inputCanvas = Instantiate(canvasPrefab, this.transform);
            }

            EasyUIScreenJoypad[] joypads = _inputCanvas.GetComponentsInChildren<EasyUIScreenJoypad>(true);

            foreach (EasyUIScreenJoypad joypad in joypads)
            {
                if (_joypadLeft == null && joypad.ScreenSide == ScreenJoypadDefaultScreenSide.Left)
                    _joypadLeft = joypad;
                else if (_joypadRight == null && joypad.ScreenSide == ScreenJoypadDefaultScreenSide.Right)
                    _joypadRight = joypad;
            }

            _setupComplete = true;
        }

        public void SetJoypadEnabledState(ScreenJoypadDefaultScreenSide screenSide, bool enabled)
        {
            if (_joypadLeft == null || _joypadRight == null)
                Initialize();

            if (screenSide == ScreenJoypadDefaultScreenSide.Left) _joypadLeft.gameObject.SetActive(enabled);
            else _joypadRight.gameObject.SetActive(enabled);
        }

        public void UpdateJoypadParameters(ScreenJoypadDefaultScreenSide screenSide, EasyUIJoypadInputParameters parameters)
        {
            if (_joypadLeft == null || _joypadRight == null)
                Initialize();

            if (screenSide == ScreenJoypadDefaultScreenSide.Left) _joypadLeft.Parameters = parameters;
            else _joypadRight.Parameters = parameters;
        }

        public void UpdateJoypadVisualElement(ScreenJoypadDefaultScreenSide screenSide, ScreenJoypadElementBaseType type, EasyUIJoypadVisualElement element)
        {
            if (_joypadLeft == null || _joypadRight == null)
                Initialize();

            if (screenSide == ScreenJoypadDefaultScreenSide.Left) _joypadLeft.ReplaceVisualElement(type, element);
            else _joypadRight.ReplaceVisualElement(type, element);
        }
    }
}
