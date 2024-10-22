using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.EasyUI.ScreenInput;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;
using Mona.SDK.Brains.EasyUI.ScreenInput.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnVirtualJoypadInstructionTile : InstructionTile, IInstructionTileWithPreload, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnVirtualJoypad";
        public const string NAME = "On Virtual Joypad";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnVirtualJoypadInstructionTile);

        [SerializeField] private ScreenJoypadDefaultScreenSide _screenSide = ScreenJoypadDefaultScreenSide.Left;
        [BrainPropertyEnum(true)] public ScreenJoypadDefaultScreenSide ScreenSide { get => _screenSide; set => _screenSide = value; }

        [SerializeField] private ScreenJoypadInteractionState _state = ScreenJoypadInteractionState.Down;
        [BrainPropertyEnum(true)] public ScreenJoypadInteractionState State { get => _state; set => _state = value; }


        private IMonaBrain _brain;
        private EasyUIScreenInput _screenInput;

        public OnVirtualJoypadInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            _screenInput = MonaGlobalBrainRunner.Instance.ScreenInput;
            _screenInput.Initialize();
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            ScreenJoypadInteractionState currentState = _screenInput.GetJoypadState(_screenSide);

            return currentState == _state ? Complete(InstructionTileResult.Success) : Complete(InstructionTileResult.Failure);
        }
    }
}