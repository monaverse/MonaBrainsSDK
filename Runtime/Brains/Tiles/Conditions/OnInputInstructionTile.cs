using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInputInstructionTile : InputInstructionTile
    {
        public const string ID = "OnInput";
        public const string NAME = "Mona Input";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnInputInstructionTile);

        [SerializeField]
        private MonaInputType _inputType = MonaInputType.Action;
        [BrainPropertyEnum(true)]
        public MonaInputType InputType { get => _inputType; set => _inputType = value; }

        [SerializeField]
        private MonaInputState _inputState = MonaInputState.Pressed;
        [BrainPropertyEnum(true)]
        public MonaInputState InputState { get => _inputState; set => _inputState = value; }

        [SerializeField]
        private MonaInputMoveType _moveDirection = MonaInputMoveType.AllDirections;
        [BrainPropertyEnum(true)]
        [BrainPropertyShow(nameof(InputType), (int)MonaInputType.Move)]
        [BrainPropertyShow(nameof(InputType), (int)MonaInputType.Look)]
        public MonaInputMoveType MoveDirection { get => _moveDirection; set => _moveDirection = value; }

        [SerializeField] private bool _useLookAsMove;
        [BrainPropertyShow(nameof(InputType), (int)MonaInputType.Look)]
        [BrainProperty(true)] public bool UseLookAsMove { get => _useLookAsMove; set => _useLookAsMove = value; }

        [SerializeField] private bool _allowTouch;
        [BrainProperty(false)] public bool AllowTouch { get => _allowTouch; set => _allowTouch = value; }

        [SerializeField] private float _gestureTimeout;
        [BrainPropertyShow(nameof(AllowTouch), true)]
        [BrainProperty(false)] public float GestureTimeout { get => _gestureTimeout; set => _gestureTimeout = value; }

        [SerializeField] private ZoneSizeType _joystickType = ZoneSizeType.ScreenPercentage;
        [BrainPropertyShow(nameof(AllowTouch), true)]
        [BrainPropertyEnum(false)] public ZoneSizeType JoystickType { get => _joystickType; set => _joystickType = value; }

        [SerializeField] private float _joystickSize = 30;
        [BrainPropertyShow(nameof(AllowTouch), true)]
        [BrainProperty(false)] public float JoystickSize
        {
            get
            {
                switch (_joystickType)
                {
                    case ZoneSizeType.ScreenPercentage:
                        if (_joystickSize < 5f || _joystickSize > 95) _joystickSize = 30f;
                        break;

                    case ZoneSizeType.Resolution:
                        if (_joystickSize < 5f) _joystickSize = 500f;
                        break;
                }

                return _joystickSize;
            }
            set
            {
                switch (_joystickType)
                {
                    case ZoneSizeType.ScreenPercentage:
                        if (value < 5f || value > 95) value = 30f;
                        break;

                    case ZoneSizeType.Resolution:
                        if (value < 5f) value = 500f;
                        break;
                }

                _joystickSize = value;
            }
        }

        [SerializeField] private ZoneSizeType _deadZoneType = ZoneSizeType.ScreenPercentage;
        [BrainPropertyShow(nameof(AllowTouch), true)]
        [BrainPropertyEnum(false)] public ZoneSizeType DeadZoneType { get => _deadZoneType; set => _deadZoneType = value; }

        [SerializeField] private float _joystickSizeDeadZone = 15;
        [BrainPropertyShow(nameof(AllowTouch), true)]
        [BrainProperty(false)] public float JoystickDeadZone
        {
            get
            {
                switch (_deadZoneType)
                {
                    case ZoneSizeType.ScreenPercentage:
                        if (_joystickSizeDeadZone < 1f || _joystickSizeDeadZone > 95f) _joystickSizeDeadZone = 15f;
                        break;
                    case ZoneSizeType.Resolution:
                        Mathf.Round(_joystickSizeDeadZone);
                        if (_joystickSizeDeadZone < 1f) _joystickSizeDeadZone = 100f;
                        break;
                }

                return _joystickSizeDeadZone;
            }
            set
            {
                switch (_deadZoneType)
                {
                    case ZoneSizeType.ScreenPercentage:
                        if (value < 1f || value > 95f) value = 15f;
                        break;
                    case ZoneSizeType.Resolution:
                        Mathf.Round(value);
                        if (value < 1f) value = 100f;
                        break;
                }

                _joystickSizeDeadZone = value;
            }
        }

        private float TrueJoystickSize => _joystickType == ZoneSizeType.Resolution ? _joystickSize : GetPixelSizeFromPercentage(_joystickSize);
        private float TrueDeadZone => _deadZoneType == ZoneSizeType.Resolution ? _joystickSizeDeadZone : GetPixelSizeFromPercentage(_joystickSizeDeadZone);

        public enum ZoneSizeType
        {
            ScreenPercentage,
            Resolution
        }

        protected override MonaInputState GetInputState() => _inputState;

        private float _mouseLookSensitivity = 30f;

        protected MonaInput _bodyInput;

        private int GetPixelSizeFromPercentage(float percentage)
        {
            percentage *= 0.1f;
            float width = Screen.width;
            float height = Screen.height;

            float smallestResolution = width > height ? height : width;

            return Mathf.RoundToInt(smallestResolution * percentage);
        }

        protected override void ProcessLocalInput()
        {
            if (BrainOnRemotePlayer()) return;

            //if (_joystickSize == 0)
            //    _joystickSize = 500f;

            //if (_joystickSizeDeadZone == 0)
            //    _joystickSizeDeadZone = _joystickSize * .1f;

            //if (_useLookAsMove)
            //    Cursor.lockState = CursorLockMode.Locked;

            var localInput = _brainInput.ProcessInput(_brain.LoggingEnabled, _inputType, GetInputState(), _allowTouch, _gestureTimeout, _joystickSize, _joystickSizeDeadZone);
            if (localInput.GetButton(_inputType) == _inputState)
            {
                //Debug.Log($"{nameof(OnInputInstructionTile)} {_inputType} {_inputState}");
                SetLocalInput(localInput);
            }
        }

        private bool _xDown = false;
        private bool _yDown = false;
        private int _axis = 0;

        protected override void HandleBodyInput(MonaInputEvent evt)
        {
            _bodyInput = evt.Input;
            var input = _bodyInput.MoveValue;
            if (_inputType == MonaInputType.Look)
                input = _bodyInput.LookValue;
            
            var xDown = Mathf.Abs(input.x) > TrueDeadZone / TrueJoystickSize;
            var yDown = Mathf.Abs(input.y) > TrueDeadZone / TrueJoystickSize;

            if (!xDown) input.x = 0;
            if (!yDown) input.y = 0;

            bool shouldClear = false;

            switch (_moveDirection)
            {
                case MonaInputMoveType.FourWay:

                    if (Mathf.Abs(input.x) == Mathf.Abs(input.y))
                    {
                        if (xDown && !_xDown)
                            _axis = 1;
                        else if (yDown && !_yDown)
                            _axis = 2;
                    }
                    else if (Mathf.Abs(input.x) > Mathf.Abs(input.y) && xDown)
                        _axis = 1;
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x) && yDown)
                        _axis = 2;
                    else
                        _axis = 0;

                    if (_axis == 1)
                        input.y = 0;
                    else if (_axis == 2)
                        input.x = 0;
                    else
                        input.x = input.y = 0;

                    _xDown = xDown;
                    _yDown = yDown;

                    break;

                case MonaInputMoveType.EightWay:

                    if (Mathf.Abs(input.x) == Mathf.Abs(input.y))
                    {
                        if (xDown && !_xDown)
                            _axis = 3;
                        else if (yDown && !_yDown)
                            _axis = 4;
                    }
                    else if (Mathf.Abs(input.x) > Mathf.Abs(input.y) && xDown)
                        _axis = 1;
                    else if (Mathf.Abs(input.y) > Mathf.Abs(input.x) && yDown)
                        _axis = 2;
                    else
                        _axis = 0;

                    if (_axis == 1)
                        input.y = 0;
                    else if (_axis == 2)
                        input.x = 0;
                    else if (_axis == 3)
                        input.y = Mathf.Abs(input.x) * Mathf.Sign(input.y);
                    else if (_axis == 4)
                        input.x = Mathf.Abs(input.y) * Mathf.Sign(input.x);
                    else
                        input.x = input.y = 0;

                    if(input.magnitude > 1f)
                        input.Normalize();

                    _xDown = xDown;
                    _yDown = yDown;

                    break;
                case MonaInputMoveType.Horizontal:
                    input.y = 0;
                    if (Mathf.Approximately(input.x, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.Vertical:
                    input.x = 0;
                    if (Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.Left:
                    input.y = 0;
                    if (input.x > 0) input.x = 0;
                    if (Mathf.Approximately(input.x, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.Right:
                    input.y = 0;
                    if (input.x < 0) input.x = 0;
                    if (Mathf.Approximately(input.x, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.Up:
                    input.x = 0;
                    if (input.y < 0) input.y = 0;
                    if (Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.Down:
                    input.x = 0;
                    if (input.y > 0) input.y = 0;
                    if (Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.UpLeft:
                    input = LockDiagonal(input);
                    if (input.x > 0 || input.y < 0) input.x = input.y = 0;
                    if (Mathf.Approximately(input.x, 0f) && Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.UpRight:
                    input = LockDiagonal(input);
                    if (input.x < 0 || input.y < 0) input.x = input.y = 0;
                    if (Mathf.Approximately(input.x, 0f) && Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.DownLeft:
                    input = LockDiagonal(input);
                    if (input.x > 0 || input.y > 0) input.x = input.y = 0;
                    if (Mathf.Approximately(input.x, 0f) && Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                case MonaInputMoveType.DownRight:
                    input = LockDiagonal(input);
                    if (input.x < 0 || input.y > 0) input.x = input.y = 0;
                    if (Mathf.Approximately(input.x, 0f) && Mathf.Approximately(input.y, 0f))
                        shouldClear = true;
                    break;
                default:
                    //do nothing;
                    break;
            }

            //Debug.Log($"{nameof(OnInputInstructionTile)} {_inputType} {_moveDirection} {input} {xDown} {yDown}");

            if (shouldClear)
            {
                ClearInput();
            }
            else
            {
                if (_inputType == MonaInputType.Look && !_useLookAsMove)
                    _bodyInput.LookValue = input;
                else
                    _bodyInput.MoveValue = input;

                //Debug.Log($"oninput: move: {_bodyInput.MoveValue}");
                _instruction.InstructionInput = _bodyInput;
            }
        }

        private Vector2 LockDiagonal(Vector2 input)
        {
            var xDown = Mathf.Abs(input.x) > TrueDeadZone / JoystickSize;
            var yDown = Mathf.Abs(input.y) > TrueDeadZone / JoystickSize;

            if (!xDown || !yDown)
                input.x = input.y = 0;
            else
            {
                var sum = Mathf.Abs(input.x) + Mathf.Abs(input.y);
                input.x = sum * Mathf.Sign(input.x);
                input.y = sum * Mathf.Sign(input.y);
            }

            if (input.magnitude > 1f)
                input.Normalize();

            return input;
        }

        public override void ReprocessInput(MonaInput input)
        {
            SetLocalInput(input);
        }

        public override void ClearInput()
        {
            _bodyInput = default;
        }

        public override MonaInput GetInput()
        {
            return _bodyInput;
        }

        public override InstructionTileResult Do()
        {
            if (_bodyInput.GetButton(_inputType) == _inputState)
            {
                //if(_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnInputInstructionTile)} DO {_inputType} {_inputState} {Time.frameCount}");
                //if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnInputInstructionTile)}.{nameof(Do)} input active {_inputType} {_inputState}");
                _bodyInput = default;
                return Complete(InstructionTileResult.Success);
            }            
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}