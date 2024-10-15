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
using Mona.SDK.Core.Assets.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.UI
{
    [Serializable]
    public class SetJoypadVisualsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "SetJoypadVisuals";
        public const string NAME = "Set Joypad Visuals";
        public const string CATEGORY = "User Interface";
        public override Type TileType => typeof(SetJoypadVisualsInstructionTile);

        [SerializeField] private ScreenJoypadDefaultScreenSide _screenSide = ScreenJoypadDefaultScreenSide.Left;
        [BrainPropertyEnum(true)] public ScreenJoypadDefaultScreenSide ScreenSide { get => _screenSide; set => _screenSide = value; }

        [SerializeField] private ScreenJoypadElementBaseType _element = ScreenJoypadElementBaseType.Background;
        [BrainPropertyEnum(true)] public ScreenJoypadElementBaseType Element { get => _element; set => _element = value; }

        [SerializeField] private string _sprite = null;
        [BrainPropertyMonaAsset(typeof(IMonaTextureAssetItem))] public string Sprite { get => _sprite; set => _sprite = value; }

        [SerializeField] private Color _value = Color.white;
        [BrainProperty(false)] public Color Value { get => _value; set => _value = value; }

        [SerializeField] private ScreenJoypadElementDisplayType _displayType = ScreenJoypadElementDisplayType.AlwaysEnabled;
        [BrainPropertyEnum(false)] public ScreenJoypadElementDisplayType DisplayType { get => _displayType; set => _displayType = value; }

        [SerializeField] private ScreenJoypadElementSize _sizeType = ScreenJoypadElementSize.Percentage;
        [BrainPropertyEnum(false)] public ScreenJoypadElementSize SizeType { get => _sizeType; set => _sizeType = value; }

        [SerializeField] private float _sizePercentage;
        [SerializeField] private string _sizePercentageName;
        [BrainProperty(true)] public float SizePercentage { get => _sizePercentage; set => _sizePercentage = value; }
        [BrainPropertyValueName("SizePercentage", typeof(IMonaVariablesFloatValue))] public string SizePercentageName { get => _sizePercentageName; set => _sizePercentageName = value; }

        public SetJoypadVisualsInstructionTile() { }

        private IMonaBrain _brain;
        private EasyUIScreenInput _screenInput;

        //public UIDisplayType GeneralDisplay => _enableInput ? UIDisplayType.Show : UIDisplayType.Hide;
        //public UIDisplayType AnalogDisplay => _enableInput && _inputType == ScreenJoypadInputType.Analog ? UIDisplayType.Show : UIDisplayType.Hide;
        //public UIDisplayType DigitalDisplay => _enableInput && _inputType == ScreenJoypadInputType.Digital ? UIDisplayType.Show : UIDisplayType.Hide;
        //public UIDisplayType TrackingDisplay => _enableInput && ((_inputType == ScreenJoypadInputType.Analog && _analogPlacement == ScreenJoypadPlacementType.Tracking) || (_inputType == ScreenJoypadInputType.Digital && _digitalPlacement == ScreenJoypadPlacementType.Tracking)) ? UIDisplayType.Show : UIDisplayType.Hide;

        //private ScreenJoypadAxisType InputAxes => _inputType == ScreenJoypadInputType.Analog ? _analogAxes : _digitalAxes;
        //private ScreenJoypadPlacementType InputPlacement => _inputType == ScreenJoypadInputType.Analog ? _analogPlacement : _digitalPlacement;
        //private float InputDeadZone => _inputType == ScreenJoypadInputType.Analog ? _analogDeadZone : _digitalDeadZone;


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

            //if (!string.IsNullOrEmpty(_enableInputName))
            //    _enableInput = _brain.Variables.GetBool(_enableInputName);

            //if (!string.IsNullOrEmpty(_analogDeadZoneName))
            //    _analogDeadZone = _brain.Variables.GetFloat(_analogDeadZoneName);

            //if (!string.IsNullOrEmpty(_digitalDeadZoneName))
            //    _digitalDeadZone = _brain.Variables.GetFloat(_digitalDeadZoneName);

            //if (!string.IsNullOrEmpty(_handleExtentsName))
            //    _handleExtents = _brain.Variables.GetFloat(_handleExtentsName);

            //if (!string.IsNullOrEmpty(_trackingThresholdName))
            //    _trackingThreshold = _brain.Variables.GetFloat(_trackingThresholdName);

            //if (HasVector2Values(_startLocationName))
            //    _startLocation = GetVector2Value(_brain, _startLocationName);

            //if (!_enableInput)
            //    _screenInput.SetJoypadEnabledState(_screenSide, _enableInput);

            //EasyUIJoypadInputParameters parameters = new EasyUIJoypadInputParameters
            //{
            //    Enabled = _enableInput,
            //    InputType = _inputType,
            //    Axes = InputAxes,
            //    ControlType = _controlType,
            //    Placement = InputPlacement,
            //    MoveMagnitude = _movementMagnitude,
            //    Visiblity = _visiblity,
            //    DeadZone = InputDeadZone,
            //    HandleExtents = _handleExtents,
            //    TrackingThreshold = _trackingThreshold,
            //    StartLocation = _startLocation
            //};

            //_screenInput.UpdateJoypadParameters(_screenSide, parameters);

            return Complete(InstructionTileResult.Success);
        }
    }
}