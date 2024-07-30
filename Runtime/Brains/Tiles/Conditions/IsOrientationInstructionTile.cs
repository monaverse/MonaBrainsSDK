using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class IsOrientationInstructionTile : InstructionTile, IInstructionTileWithPreload, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "IsOrientation";
        public const string NAME = "Screen Orientation";
        public const string CATEGORY = "System";
        public override Type TileType => typeof(IsOrientationInstructionTile);

        [SerializeField] private BrainsScreenOrientation _orientation = BrainsScreenOrientation.LandscapeLeft;
        [BrainPropertyEnum(true)] public BrainsScreenOrientation Orientation { get => _orientation; set => _orientation = value; }

        [SerializeField] private bool _onChangeOnly = false;
        [BrainProperty(true)] public bool OnChangeOnly { get => _onChangeOnly; set => _onChangeOnly = value; }

        private IMonaBrain _brain;
        private bool _tileIsTrue;
        private bool _lastTileValue;
        private bool _change;

        public enum BrainsScreenOrientation
        {
            Landscape = 0,
            LandscapeLeft = 10,
            LandscapeRight = 20,
            Portrait = 30
        }

        public IsOrientationInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
           
        }

        private bool IsTrue()
        {
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                if (_orientation == BrainsScreenOrientation.Portrait)
                    return Screen.orientation == ScreenOrientation.Portrait;
                else if (_orientation == BrainsScreenOrientation.Landscape)
                    return Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
                else if (_orientation == BrainsScreenOrientation.LandscapeLeft)
                    return Screen.orientation == ScreenOrientation.LandscapeLeft;
                else if (_orientation == BrainsScreenOrientation.LandscapeRight)
                    return Screen.orientation == ScreenOrientation.LandscapeRight;
            }
            else
            {
                if (_orientation == BrainsScreenOrientation.Portrait)
                    return Screen.width < Screen.height;
                else if (_orientation == BrainsScreenOrientation.Landscape)
                    return Screen.width >= Screen.height;
                else if (_orientation == BrainsScreenOrientation.LandscapeLeft)
                    return Screen.width >= Screen.height;
                else if (_orientation == BrainsScreenOrientation.LandscapeRight)
                    return Screen.width >= Screen.height;
            }
            return false;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _tileIsTrue = IsTrue();

            if(_onChangeOnly)
            {
                if(_tileIsTrue != _lastTileValue)
                {
                    _change = true;
                    _lastTileValue = _tileIsTrue;
                }

                if (_tileIsTrue && _change)
                {
                    _change = false;
                    return Complete(InstructionTileResult.Success);
                }

                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
            }

            return _tileIsTrue ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}