using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Core.Utils;
using Mona.SDK.Core;
using Unity.VisualScripting;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnMouseInstructionTile :
        InstructionTile, IInstructionTileWithPreload, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnMouse";
        public const string NAME = "On Mouse";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnMouseInstructionTile);

        [SerializeField] private MouseInputType _inputType = MouseInputType.Button;
        [BrainPropertyEnum(true)] public MouseInputType InputType { get => _inputType; set => _inputType = value; }

        [SerializeField] private MouseButtonType _button = MouseButtonType.Left;
        [BrainPropertyShow(nameof(InputType), (int)MouseInputType.Button)]
        [BrainPropertyEnum(true)] public MouseButtonType Button { get => _button; set => _button = value; }

        [SerializeField] private MonaInputState _buttonState = MonaInputState.Pressed;
        [BrainPropertyShow(nameof(InputType), (int)MouseInputType.Button)]
        [BrainPropertyEnum(true)] public MonaInputState ButtonState { get => _buttonState; set => _buttonState = value; }

        [SerializeField] private MouseMovementAxisType _movementAxis = MouseMovementAxisType.Any;
        [BrainPropertyShow(nameof(InputType), (int)MouseInputType.Move)]
        [BrainPropertyEnum(true)] public MouseMovementAxisType MovementAxis { get => _movementAxis; set => _movementAxis = value; }

        [SerializeField] private WheelAxisType _wheelAxis = WheelAxisType.Any;
        [BrainPropertyShow(nameof(InputType), (int)MouseInputType.Wheel)]
        [BrainPropertyEnum(true)] public WheelAxisType WheelAxis { get => _wheelAxis; set => _wheelAxis = value; }

        [SerializeField] private float _threshold = 0.1f;
        [SerializeField] private string _thresholdName;
        [BrainPropertyShow(nameof(InputType), (int)MouseInputType.Move)]
        [BrainProperty(true)] public float Threshold { get => _threshold; set => _threshold = value; }
        [BrainPropertyValueName("Threshold", typeof(IMonaVariablesFloatValue))] public string ThresholdName { get => _thresholdName; set => _thresholdName = value; }

        private IMonaBrain _brain;

        public OnMouseInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_thresholdName))
                _threshold = _brain.Variables.GetFloat(_thresholdName);

            bool success = false;

            switch (_inputType)
            {
                case MouseInputType.Button:
                    success = GetButtonState();
                    break;
                case MouseInputType.Move:
                    success = GetMoveState();
                    break;
                case MouseInputType.Wheel:
                    success = GetWheelState();
                    break;
            }

            if (success)
                return Complete(InstructionTileResult.Success);
            else
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool GetButtonState()
        {
            switch (_buttonState)
            {
                case MonaInputState.Pressed:
                    return Input.GetMouseButtonDown((int)_button);
                case MonaInputState.Up:
                    return Input.GetMouseButtonUp((int)_button);
                case MonaInputState.Held:
                    return Input.GetMouseButton((int)_button);
            }

            return true;
        }

        private bool GetMoveState()
        {
            float h = Input.GetAxis("Mouse X");
            float v = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(h) < _threshold)
                h = 0f;
            if (Mathf.Abs(v) < _threshold)
                v = 0f;

            switch (_movementAxis)
            {
                case MouseMovementAxisType.Any:
                    return !Mathf.Approximately(h, 0) || !Mathf.Approximately(v, 0);
                case MouseMovementAxisType.Horizontal:
                    return !Mathf.Approximately(h, 0);
                case MouseMovementAxisType.Vertical:
                    return !Mathf.Approximately(v, 0);
                case MouseMovementAxisType.Up:
                    return v > 0;
                case MouseMovementAxisType.Down:
                    return v < 0;
                case MouseMovementAxisType.Left:
                    return h < 0;
                case MouseMovementAxisType.Right:
                    return h > 0;
            }

            return false;
        }

        private bool GetWheelState()
        {
            float y = Input.GetAxis("Mouse ScrollWheel");

            switch (_wheelAxis)
            {
                case WheelAxisType.Any:
                    return !Mathf.Approximately(y, 0);
                case WheelAxisType.WheelUp:
                    return y > 0;
                case WheelAxisType.WheelDown:
                    return y < 0;
            }

            return false;
        }
    }
}