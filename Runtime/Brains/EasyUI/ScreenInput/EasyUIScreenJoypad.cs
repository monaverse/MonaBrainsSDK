using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;
using Mona.SDK.Brains.EasyUI.ScreenInput.Structs;

namespace Mona.SDK.Brains.EasyUI.ScreenInput
{
    public class EasyUIScreenJoypad : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public ScreenJoypadDefaultScreenSide ScreenSide = ScreenJoypadDefaultScreenSide.Left;
        [SerializeField] private EasyUIScreenJoypad _opposingJoypad;
        [SerializeField] private EasyUIJoypadInputParameters _parameters = new EasyUIJoypadInputParameters();
        [SerializeField] private EasyUIJoypadInputVisuals _visuals = new EasyUIJoypadInputVisuals();
        [SerializeField] private RectTransform _rootRect;
        [SerializeField] private RectTransform _backgroundRect;
        [SerializeField] private RectTransform _handleRect;
        [SerializeField] private RectTransform _pointerRect;

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _handleImage;
        [SerializeField] private Image _pointerImage;

        private Vector2 _inputVector = Vector2.zero;
        private RectTransform _baseRect;
        private Canvas _inputCanvas;
        private Camera _camera;
        private MonaBrainInput _brainInput;
        private UserInputState _currentUserInputState = UserInputState.Idle;

        private readonly float _referenceElementPixels = 256f;
        private readonly float _referenceScreenPixels = 1080f;

        public Vector2 InputVector => _inputVector;
        private EasyUIJoypadBaseVisuals CurrentBaseVisuals => _visuals.GetVisuals(_parameters.InputType, _parameters.Axes);

        private ScreenJoypadBaseControlType ControlType
        {
            get
            {
                if (_parameters.ControlType != ScreenJoypadBaseControlType.Default)
                    return _parameters.ControlType;

                return ScreenSide == ScreenJoypadDefaultScreenSide.Left ? ScreenJoypadBaseControlType.Movement : ScreenJoypadBaseControlType.Look;
            }
        }

        public EasyUIJoypadInputParameters Parameters
        {
            get { return _parameters; }
            set
            {
                bool updateDefaultPosition = _parameters.StartLocation != value.StartLocation;

                _parameters = value;
                gameObject.SetActive(_parameters.Enabled);

                SetScreenArea();

                if (updateDefaultPosition)
                    StartCoroutine(SetDefaultPosition());
                else
                    UpdateVisuals();
            }
        }

        public EasyUIJoypadInputVisuals Visuals
        {
            get { return _visuals; }
            set
            {
                Visuals = value;
                UpdateVisuals();
            }
        }

        private enum UserInputState
        {
            Down = 0,
            Drag = 10,
            Up = 20,
            Idle = 30
        }

        private static readonly Vector2[] directions = new Vector2[]
        {
            Vector2.up,                      // Up
            new Vector2(1, 1).normalized,    // Up and Right
            Vector2.right,                   // Right
            new Vector2(1, -1).normalized,   // Down and Right
            Vector2.down,                    // Down
            new Vector2(-1, -1).normalized,  // Down and Left
            Vector2.left,                    // Left
            new Vector2(-1, 1).normalized    // Up and Left
        };

        private void Awake()
        {
            _baseRect = GetComponent<RectTransform>();
            _inputCanvas = GetComponentInParent<Canvas>();
        }

        private void OnEnable()
        {
            if (_brainInput == null && MonaGlobalBrainRunner.Instance != null)
                _brainInput = MonaGlobalBrainRunner.Instance.GetBrainInput();

            SetScreenArea();
            SetOpposingScreenArea();
            Vector2 center = new Vector2(0.5f, 0.5f);
            _rootRect.pivot = center;
            _handleRect.anchorMin = center;
            _handleRect.anchorMax = center;
            _handleRect.pivot = center;
            _handleRect.anchoredPosition = Vector2.zero;

            StartCoroutine(SetDefaultPosition());
        }

        private void OnDisable()
        {
            _inputVector = Vector2.zero;

            SetBrainInput();
            SetOpposingScreenArea(true);
            _backgroundImage.enabled = false;
            _handleImage.enabled = false;
            _pointerImage.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _currentUserInputState = UserInputState.Drag;

            if (!_parameters.Enabled)
                return;

            _camera = null;
            if (_inputCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                _camera = _inputCanvas.worldCamera;

            Vector2 position = RectTransformUtility.WorldToScreenPoint(_camera, _rootRect.position);
            Vector2 radius = _rootRect.sizeDelta / 2;
            Vector2 input = (eventData.position - position) / (radius * _inputCanvas.scaleFactor);
            _inputVector = FormatInput(input, out float magnitude, out bool overcameDeadzone);

            if (_parameters.Placement == ScreenJoypadPlacementType.Tracking && input.magnitude > _parameters.TrackingThreshold)
            {
                Vector2 difference = input.normalized * (input.magnitude - _parameters.TrackingThreshold) * radius;
                _rootRect.anchoredPosition += difference;
            }

            SetBrainInput();
            MoveHandle(_inputVector, radius);
            SetPointerRotation(_inputVector, overcameDeadzone);
            UpdateActiveStates(overcameDeadzone);
            LerpVisualsWithInput(magnitude);
        }

        private void SetBrainInput()
        {
            if (_brainInput == null)
                return;

            if (ControlType == ScreenJoypadBaseControlType.Movement)
                _brainInput.ScreenMoveVector = _inputVector;
            else
                _brainInput.ScreenLookVector = _inputVector;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _currentUserInputState = UserInputState.Up;

            if (!_parameters.Enabled)
                return;

            _inputVector = Vector2.zero;
            _handleRect.anchoredPosition = Vector2.zero;

            SetBrainInput();
            UpdateActiveStates();
            LerpVisualsWithInput();
            _currentUserInputState = UserInputState.Idle;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _currentUserInputState = UserInputState.Down;

            if (!_parameters.Enabled)
                return;

            if (_parameters.Placement != ScreenJoypadPlacementType.Fixed)
                _rootRect.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);

            OnDrag(eventData);
        }

        private protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _camera, out localPoint))
            {
                Vector2 pivotOffset = _baseRect.pivot * _baseRect.sizeDelta;
                return localPoint - (_rootRect.anchorMax * _baseRect.sizeDelta) + pivotOffset;
            }

            return localPoint;
        }

        private Vector2 FormatInput(Vector2 input, out float magnitude, out bool overcameDeadzone)
        {
            magnitude = input.magnitude;
            float deadZone = _parameters.DeadZone;

            if (magnitude < _parameters.DeadZone)
            {
                magnitude = 0;
                overcameDeadzone = false;
                return Vector2.zero;
            }

            overcameDeadzone = true;

            if (magnitude > 1)
            {
                magnitude = 1;
                input = input.normalized;
            }
            else if (_parameters.MoveMagnitude == ScreenJoypadMoveMagnitudeType.StartAtDeadzone)
            {
                Vector2 normalizedInput = input / magnitude;
                magnitude = (magnitude - deadZone) / (1 - deadZone);
                input = normalizedInput * magnitude;
            }

            float x = input.x;
            float y = input.y;

            if (_parameters.InputType == ScreenJoypadInputType.Digital && _parameters.Axes != ScreenJoypadAxisType.All)
            {
                if (!Mathf.Approximately(x, 0f)) x = Mathf.Sign(x);
                if (!Mathf.Approximately(y, 0f)) y = Mathf.Sign(y);
            }

            switch (_parameters.Axes)
            {
                case ScreenJoypadAxisType.All:
                    return _parameters.InputType == ScreenJoypadInputType.Digital ? input.normalized : new Vector2(x, y);

                case ScreenJoypadAxisType.EightWay:
                    if (_parameters.InputType == ScreenJoypadInputType.Digital)
                    {
                        float absX = Mathf.Abs(input.x);
                        float absY = Mathf.Abs(input.y);

                        if (absX < _parameters.DigitalDiagonalThreshold || absY < _parameters.DigitalDiagonalThreshold)
                        {
                            if (absY > absX) x = 0f;
                            else y = 0f;

                            return new Vector2(x, y);
                        }

                        return new Vector2(x, y).normalized;
                    }

                    float maxDot = -1f;
                    Vector2 closestDirection = Vector2.zero;

                    for (int i = 0; i < directions.Length; i++)
                    {
                        float dot = Vector2.Dot(input.normalized, directions[i]);
                        if (dot > maxDot)
                        {
                            maxDot = dot;
                            closestDirection = directions[i];
                        }
                    }

                    return closestDirection * input.magnitude;

                case ScreenJoypadAxisType.FourWay:
                    if (Mathf.Abs(input.y) > Mathf.Abs(input.x)) x = 0f;
                    else y = 0f;
                    break;

                case ScreenJoypadAxisType.Horizontal:
                    y = 0f;
                    break;

                case ScreenJoypadAxisType.Vertical:
                    x = 0f;
                    break;

                case ScreenJoypadAxisType.Up:
                    x = 0f;
                    y = y > 0f ? y : 0f;
                    break;

                case ScreenJoypadAxisType.Down:
                    x = 0f;
                    y = y < 0f ? y : 0f;
                    break;

                case ScreenJoypadAxisType.Left:
                    x = x < 0f ? x : 0f;
                    y = 0f;
                    break;

                case ScreenJoypadAxisType.Right:
                    x = x > 0 ? x : 0f;
                    y = 0f;
                    break;

                case ScreenJoypadAxisType.HalfCircleUp:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        y = y > 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y > 0 ? y : 0;
                    }
                    else
                    {
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.HalfCircleDown:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        y = y < 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y < 0 ? y : 0;
                    }
                    else
                    {
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.HalfCircleLeft:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x < 0f ? x : 0f;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                    }
                    else
                    {
                        x = x < 0 ? x : 0;
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.HalfCircleRight:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x > 0f ? x : 0f;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                    }
                    else
                    {
                        x = x > 0 ? x : 0;
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.UpAndLeft:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x < 0f ? x : 0f;
                        y = y > 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y > 0 ? y : 0;
                    }
                    else
                    {
                        x = x < 0f ? x : 0f;
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.UpAndRight:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x > 0f ? x : 0f;
                        y = y > 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y > 0 ? y : 0;
                    }
                    else
                    {
                        x = x > 0f ? x : 0f;
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.DownAndLeft:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x < 0f ? x : 0f;
                        y = y < 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y < 0 ? y : 0;
                    }
                    else
                    {
                        x = x < 0f ? x : 0f;
                        y = 0f;
                    }
                    break;

                case ScreenJoypadAxisType.DownAndRight:
                    if (_parameters.InputType == ScreenJoypadInputType.Analog)
                    {
                        x = x > 0f ? x : 0f;
                        y = y < 0 ? y : 0;
                    }
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
                    {
                        x = 0f;
                        y = y < 0 ? y : 0;
                    }
                    else
                    {
                        x = x > 0f ? x : 0f;
                        y = 0f;
                    }
                    break;
            }



            Vector2 newVector = new Vector2(x, y);
            magnitude = newVector.magnitude;
            return newVector;
        }

        private void MoveHandle(Vector2 finalInputVector, Vector2 radius)
        {
            _handleRect.anchoredPosition = finalInputVector * radius * _parameters.HandleExtents;
        }

        private void SetPointerRotation(Vector2 finalInputVector, bool overcameDeadzone)
        {
            bool pointerOkay = overcameDeadzone && finalInputVector != Vector2.zero;

            if (pointerOkay && !_pointerRect.gameObject.activeInHierarchy)
            {
                _pointerRect.gameObject.SetActive(true);
            }
            else if (!pointerOkay)
            {
                if (_pointerRect.gameObject.activeInHierarchy)
                    _pointerRect.gameObject.SetActive(false);

                return;
            }

            Vector2 direction = finalInputVector.normalized;
            float angleRadians = Mathf.Atan2(-direction.x, direction.y);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees < 0)
                angleDegrees += 360f;

            _pointerRect.localEulerAngles = new Vector3(0, 0, angleDegrees);
        }

        private void SetScreenArea(bool forceSingleInput = false)
        {
            ScreenJoypadLayout layout = _opposingJoypad != null && _opposingJoypad.gameObject.activeInHierarchy ? ScreenJoypadLayout.DualInputs : ScreenJoypadLayout.SingleInput;

            if (layout == ScreenJoypadLayout.SingleInput || forceSingleInput)
            {
                _baseRect.anchorMin = new Vector2(0f, 0f);
                _baseRect.anchorMax = new Vector2(1f, 1f);
                _baseRect.pivot = Vector2.zero;
            }
            else
            {
                switch (ScreenSide)
                {
                    case ScreenJoypadDefaultScreenSide.Left:
                        _baseRect.anchorMin = new Vector2(0f, 0f);
                        _baseRect.anchorMax = new Vector2(0.5f, 1f);
                        _baseRect.pivot = Vector2.zero;
                        break;
                    case ScreenJoypadDefaultScreenSide.Right:
                        _baseRect.anchorMin = new Vector2(0.5f, 0f);
                        _baseRect.anchorMax = new Vector2(1f, 1f);
                        _baseRect.pivot = Vector2.zero;
                        break;
                }
            }
        }

        private void SetOpposingScreenArea(bool forceSingleInput = false)
        {
            if (_opposingJoypad != null && _opposingJoypad.isActiveAndEnabled)
                _opposingJoypad.SetScreenArea(forceSingleInput);
        }

        private IEnumerator SetDefaultPosition()
        {
            yield return new WaitForEndOfFrame();

            float width = Screen.width;
            float height = Screen.height;
            float x = 256f;
            float y = 256f;

            switch (ScreenSide)
            {
                case ScreenJoypadDefaultScreenSide.Left:
                    x = width * _parameters.StartLocation.x;
                    y = height * _parameters.StartLocation.y;
                    break;
                case ScreenJoypadDefaultScreenSide.Right:
                    x = width - (width * _parameters.StartLocation.x);
                    y = height * _parameters.StartLocation.y;
                    break;
            }

            _rootRect.position = new Vector3(x, y, 1f);

            UpdateVisuals();
        }

        private void LerpVisualsWithInput(float magnitude = 0f)
        {
            EasyUIJoypadBaseVisuals visuals = CurrentBaseVisuals;

            LerpElementWithInput(visuals.Background, _backgroundRect, _backgroundImage, magnitude);
            LerpElementWithInput(visuals.Handle, _handleRect, _handleImage, magnitude);
            LerpElementWithInput(visuals.Pointer, _pointerRect, _pointerImage, magnitude);
        }

        private void LerpElementWithInput(EasyUIJoypadVisualElement element, RectTransform elemeentRect, Image elementImage, float magnitude = 0f)
        {
            if (element.ElementSprite == null)
                return;

            if (element.InputScalesElement == ScreenJoypadScaleWithMagnitudeType.ChangeWithMagnitude)
            {
                elemeentRect.sizeDelta = GetElementSizeDelta(element) * Mathf.Lerp(element.MinScalePercentage, 1f, magnitude);
                element.ScaleWasLerped = true;
            }
            else if (element.ScaleWasLerped)
            {
                elemeentRect.sizeDelta = GetElementSizeDelta(element);
                element.ScaleWasLerped = false;
            }

            if (element.InputFadesElement == ScreenJoypadScaleWithMagnitudeType.ChangeWithMagnitude)
            {
                float percentage = Mathf.Lerp(element.MinFadePercentage, element.ElementColor.a, magnitude);
                Color color = _pointerImage.color;
                color.a = percentage;
                elementImage.color = color;
                element.AlphaWasLerped = true;
            }
            else if (element.AlphaWasLerped)
            {
                elementImage.color = element.ElementColor;
                element.AlphaWasLerped = false;
            }
        }

        private void UpdateActiveStates(bool overcameDeadzone = false)
        {
            ScreenJoypadDisplayType visibility = _parameters.Visiblity;
            GameObject root = _rootRect.gameObject;

            if (_currentUserInputState == UserInputState.Down || _currentUserInputState == UserInputState.Drag)
            {
                switch (visibility)
                {
                    case ScreenJoypadDisplayType.AlwaysVisible:
                    case ScreenJoypadDisplayType.HideWhenIdle:
                        if (!root.activeInHierarchy)
                            root.SetActive(true);
                        break;
                    case ScreenJoypadDisplayType.AlwaysHidden:
                        if (root.activeInHierarchy)
                            root.SetActive(false);
                        return;
                }
            }
            else
            {
                switch (visibility)
                {
                    case ScreenJoypadDisplayType.AlwaysVisible:
                        if (!root.activeInHierarchy)
                            root.SetActive(true);
                        break;
                    case ScreenJoypadDisplayType.HideWhenIdle:
                    case ScreenJoypadDisplayType.AlwaysHidden:
                        if (root.activeInHierarchy)
                            root.SetActive(false);
                        return;
                }
            }

            EasyUIJoypadBaseVisuals visuals = CurrentBaseVisuals;
            UpdateElementActiveState(visuals.Background, _backgroundImage, overcameDeadzone);
            UpdateElementActiveState(visuals.Handle, _handleImage, overcameDeadzone);
            UpdateElementActiveState(visuals.Pointer, _pointerImage, overcameDeadzone);

        }

        private void UpdateElementActiveState(EasyUIJoypadVisualElement element, Image image, bool overcameDeadzone)
        {
            if (element.ElementSprite == null)
            {
                if (image.enabled)
                    image.enabled = false;
                return;
            }

            if (_currentUserInputState == UserInputState.Down || _currentUserInputState == UserInputState.Drag)
            {
                switch (element.DisplayType)
                {
                    case ScreenJoypadElementDisplayType.AlwaysEnabled:
                        if (!image.enabled) image.enabled = true;
                        return;
                    case ScreenJoypadElementDisplayType.AlwaysDisabled:
                        if (image.enabled) image.enabled = false;
                        return;
                    case ScreenJoypadElementDisplayType.EnabledOnValid:
                        image.enabled = overcameDeadzone;
                        return;
                }
            }
            else
            {
                switch (element.DisplayType)
                {
                    case ScreenJoypadElementDisplayType.AlwaysEnabled:
                        if (!image.enabled) image.enabled = true;
                        return;
                    case ScreenJoypadElementDisplayType.EnabledOnValid:
                    case ScreenJoypadElementDisplayType.AlwaysDisabled:
                        if (image.enabled) image.enabled = false;
                        return;
                }
            }
        }

        

        private void UpdateVisuals()
        {
            EasyUIJoypadBaseVisuals visuals = CurrentBaseVisuals;
            UpdateVisualElement(visuals.Background, _backgroundRect, _backgroundImage);
            UpdateVisualElement(visuals.Handle, _handleRect, _handleImage);
            UpdateVisualElement(visuals.Pointer, _pointerRect, _pointerImage);
            UpdateActiveStates();
            LerpVisualsWithInput();
        }

        private void UpdateVisualElement(EasyUIJoypadVisualElement element, RectTransform rt, Image image)
        {
            image.sprite = element.ElementSprite;
            image.color = element.ElementColor;
            rt.sizeDelta = GetElementSizeDelta(element);
        }

        private Vector2 GetElementSizeDelta(EasyUIJoypadVisualElement element)
        {
            if (element.SizeType == ScreenJoypadElementSize.Pixels)
                return new Vector2(element.SizePixels, element.SizePixels);

            float scaleMultiplier = GetScreenScaleFactor() / _referenceScreenPixels;
            float pixels = _referenceElementPixels * scaleMultiplier * element.SizePercentage;

            return new Vector2(pixels, pixels);
        }

        private float GetScreenScaleFactor()
        {
            if (_inputCanvas.renderMode == RenderMode.ScreenSpaceOverlay || (_inputCanvas.renderMode == RenderMode.ScreenSpaceCamera && _inputCanvas.worldCamera == null))
            {
                float res = Screen.width > Screen.height ? Screen.height : Screen.width;
                return res / _inputCanvas.scaleFactor;
            }
            else if (_inputCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                float res = 2f * _inputCanvas.worldCamera.orthographicSize;
                return res * _inputCanvas.scaleFactor;
            }

            return 1f;
        }

        public void ReplaceVisualElement(ScreenJoypadElementBaseType type, EasyUIJoypadVisualElement element)
        {
            EasyUIJoypadBaseVisuals visuals = CurrentBaseVisuals;

            switch (type)
            {
                case ScreenJoypadElementBaseType.Background:
                    if (element.ElementSprite == null)
                        element.ElementSprite = visuals.Background.ElementSprite;
                    visuals.Background = element;
                    UpdateVisualElement(visuals.Background, _backgroundRect, _backgroundImage);
                    break;
                case ScreenJoypadElementBaseType.Handle:
                    if (element.ElementSprite == null)
                        element.ElementSprite = visuals.Handle.ElementSprite;
                    visuals.Handle = element;
                    UpdateVisualElement(visuals.Handle, _handleRect, _handleImage);
                    break;
                case ScreenJoypadElementBaseType.Pointer:
                    if (element.ElementSprite == null)
                        element.ElementSprite = visuals.Pointer.ElementSprite;
                    visuals.Pointer = element;
                    UpdateVisualElement(visuals.Pointer, _pointerRect, _pointerImage);
                    break;
            }

            UpdateActiveStates();
            LerpVisualsWithInput();
        }
    }
}
