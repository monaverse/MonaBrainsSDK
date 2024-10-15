using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.EasyUI.ScreenInput;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;
using Mona.SDK.Brains.EasyUI.ScreenInput.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.UI
{
    [Serializable]
    public class SetScreenJoypadInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "SetScreenJoypad";
        public const string NAME = "Set Screen Joypad";
        public const string CATEGORY = "User Interface";
        public override Type TileType => typeof(SetScreenJoypadInstructionTile);

        [SerializeField] private ScreenJoypadDefaultScreenSide _screenSide = ScreenJoypadDefaultScreenSide.Left;
        [BrainPropertyEnum(true)] public ScreenJoypadDefaultScreenSide ScreenSide { get => _screenSide; set => _screenSide = value; }

        [SerializeField] private bool _enableInput = true;
        [SerializeField] private string _enableInputName;
        [BrainProperty(true)] public bool EnableInput { get => _enableInput; set => _enableInput = value; }
        [BrainPropertyValueName("EnableInput", typeof(ScreenJoypadInputType))] public string MyBoolName { get => _enableInputName; set => _enableInputName = value; }

        [SerializeField] private ScreenJoypadInputType _inputType = ScreenJoypadInputType.Analog;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(true)] public ScreenJoypadInputType InputType { get => _inputType; set => _inputType = value; }

        [SerializeField] private ScreenJoypadAxisType _analogAxes = ScreenJoypadAxisType.All;
        [BrainPropertyShow(nameof(AnalogDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadAxisType AxesAvailable { get => _analogAxes; set => _analogAxes = value; }

        [SerializeField] private ScreenJoypadAxisType _digitalAxes = ScreenJoypadAxisType.FourWay;
        [BrainPropertyShow(nameof(DigitalDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadAxisType DigitalAxes { get => _digitalAxes; set => _digitalAxes = value; }

        [SerializeField] private ScreenJoypadPlacementType _analogPlacement = ScreenJoypadPlacementType.CenterOnTap;
        [BrainPropertyShow(nameof(AnalogDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadPlacementType AnalogPlacement { get => _analogPlacement; set => _analogPlacement = value; }

        [SerializeField] private ScreenJoypadPlacementType _digitalPlacement = ScreenJoypadPlacementType.Fixed;
        [BrainPropertyShow(nameof(AnalogDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadPlacementType DigitalPlacement { get => _digitalPlacement; set => _digitalPlacement = value; }

        [SerializeField] private ScreenJoypadMoveMagnitudeType _movementMagnitude = ScreenJoypadMoveMagnitudeType.StartAtDeadzone;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadMoveMagnitudeType MovementMagnitude { get => _movementMagnitude; set => _movementMagnitude = value; }

        [SerializeField] private ScreenJoypadDisplayType _visiblity = ScreenJoypadDisplayType.AlwaysVisible;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadDisplayType Visiblity { get => _visiblity; set => _visiblity = value; }

        [SerializeField] private float _trackingThreshold = 1f;
        [SerializeField] private string _trackingThresholdName;
        [BrainPropertyShow(nameof(TrackingDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float TrackingThreshold { get => _trackingThreshold; set => _trackingThreshold = value; }
        [BrainPropertyValueName("TrackingThreshold", typeof(IMonaVariablesFloatValue))] public string TrackingThresholdName { get => _trackingThresholdName; set => _trackingThresholdName = value; }

        [SerializeField] private float _analogDeadZone = 0.05f;
        [SerializeField] private string _analogDeadZoneName;
        [BrainPropertyShow(nameof(AnalogDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float AnalogDeadZone { get => _analogDeadZone; set => _analogDeadZone = value; }
        [BrainPropertyValueName("AnalogDeadZone", typeof(IMonaVariablesFloatValue))] public string AnalogDeadZoneName { get => _analogDeadZoneName; set => _analogDeadZoneName = value; }

        [SerializeField] private float _digitalDeadZone = 0.25f;
        [SerializeField] private string _digitalDeadZoneName;
        [BrainPropertyShow(nameof(DigitalDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float DigitalDeadZone { get => _digitalDeadZone; set => _digitalDeadZone = value; }
        [BrainPropertyValueName("DigitalDeadZone", typeof(IMonaVariablesFloatValue))] public string DigitalDeadZoneName { get => _digitalDeadZoneName; set => _digitalDeadZoneName = value; }

        [SerializeField] private float _handleExtents = 1f;
        [SerializeField] private string _handleExtentsName;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float HandleExtents { get => _handleExtents; set => _handleExtents = value; }
        [BrainPropertyValueName("HandleExtents", typeof(IMonaVariablesFloatValue))] public string HandleExtentsName { get => _handleExtentsName; set => _handleExtentsName = value; }

        [SerializeField] private Vector2 _startLocation = new Vector2(0.15f, 0.5f);
        [SerializeField] private string[] _startLocationName;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public Vector2 StartLocation { get => _startLocation; set => _startLocation = value; }
        [BrainPropertyValueName("StartLocation", typeof(IMonaVariablesVector2Value))] public string[] StartLocationName { get => _startLocationName; set => _startLocationName = value; }

        [SerializeField] private ScreenJoypadBaseControlType _controlType = ScreenJoypadBaseControlType.Default;
        [BrainPropertyShow(nameof(GeneralDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public ScreenJoypadBaseControlType ControlType { get => _controlType; set => _controlType = value; }

        public SetScreenJoypadInstructionTile() { }

        private IMonaBrain _brain;
        private EasyUIScreenInput _screenInput;

        public UIDisplayType GeneralDisplay => _enableInput ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType AnalogDisplay => _enableInput && _inputType == ScreenJoypadInputType.Analog ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType DigitalDisplay => _enableInput && _inputType == ScreenJoypadInputType.Digital ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType TrackingDisplay => _enableInput && ((_inputType == ScreenJoypadInputType.Analog && _analogPlacement == ScreenJoypadPlacementType.Tracking) || (_inputType == ScreenJoypadInputType.Digital && _digitalPlacement == ScreenJoypadPlacementType.Tracking)) ? UIDisplayType.Show : UIDisplayType.Hide;

        private ScreenJoypadAxisType InputAxes => _inputType == ScreenJoypadInputType.Analog ? _analogAxes : _digitalAxes;
        private ScreenJoypadPlacementType InputPlacement => _inputType == ScreenJoypadInputType.Analog ? _analogPlacement : _digitalPlacement;
        private float InputDeadZone => _inputType == ScreenJoypadInputType.Analog ? _analogDeadZone : _digitalDeadZone;


        public enum UIDisplayType
        {
            Show = 0,
            Hide = 10
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _screenInput = MonaGlobalBrainRunner.Instance.ScreenInput;
            _screenInput.Initialize();
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _screenInput == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_enableInputName))
                _enableInput = _brain.Variables.GetBool(_enableInputName);

            if (!string.IsNullOrEmpty(_analogDeadZoneName))
                _analogDeadZone = _brain.Variables.GetFloat(_analogDeadZoneName);

            if (!string.IsNullOrEmpty(_digitalDeadZoneName))
                _digitalDeadZone = _brain.Variables.GetFloat(_digitalDeadZoneName);

            if (!string.IsNullOrEmpty(_handleExtentsName))
                _handleExtents = _brain.Variables.GetFloat(_handleExtentsName);

            if (!string.IsNullOrEmpty(_trackingThresholdName))
                _trackingThreshold = _brain.Variables.GetFloat(_trackingThresholdName);

            if (HasVector2Values(_startLocationName))
                _startLocation = GetVector2Value(_brain, _startLocationName);

            if (!_enableInput)
                _screenInput.SetJoypadEnabledState(_screenSide, _enableInput);

            EasyUIJoypadInputParameters parameters = new EasyUIJoypadInputParameters
            {
                Enabled = _enableInput,
                InputType = _inputType,
                Axes = InputAxes,
                ControlType = _controlType,
                Placement = InputPlacement,
                MoveMagnitude = _movementMagnitude,
                Visiblity = _visiblity,
                DeadZone = InputDeadZone,
                HandleExtents = _handleExtents,
                TrackingThreshold = _trackingThreshold,
                StartLocation = _startLocation
            };

            _screenInput.UpdateJoypadParameters(_screenSide, parameters);

            return Complete(InstructionTileResult.Success);
        }
    }
}