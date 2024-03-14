using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnKeyInstructionTile : InputInstructionTile
    {
        public const string ID = "OnKey";
        public const string NAME = "Keyboard";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnKeyInstructionTile);

        [SerializeField] private Key _key = Key.Space;
        [BrainPropertyEnum(true)] public Key Key { get => _key; set => _key = value; }

        [SerializeField] protected MonaInputState _inputState = MonaInputState.Pressed;
        [BrainProperty(true)] public MonaInputState InputState { get => _inputState; set => _inputState = value; }

        protected override MonaInputState GetInputState() => _inputState;

        protected MonaInput _bodyInput;
        protected int _keyIndex;

        protected override void AddTickDelegate()
        {
            _keyIndex = _brainInput.StartListeningForKey(_key, this);
            base.AddTickDelegate();
        }

        protected override void RemoveTickDelegate()
        {
            base.RemoveTickDelegate();
            _brainInput.StopListeningForKey(_key, this);
        }

        protected override void ProcessLocalInput()
        {
            if (BrainOnRemotePlayer()) return;

            var localInput = _brainInput.ProcessInput(_brain.LoggingEnabled, MonaInputType.Key, GetInputState());
            if (localInput.GetKey(_keyIndex) == GetInputState())
            {
                SetLocalInput(localInput);
            }
        }

        protected override void ClearInput()
        {
            _bodyInput = default;
        }

        protected override void HandleBodyInput(MonaInputEvent evt)
        {
            _bodyInput = evt.Input;
        }

        public override void ReprocessInput(MonaInput input)
        {
            SetLocalInput(input);
        }

        public override MonaInput GetInput()
        {
            return _bodyInput;
        }

        public override InstructionTileResult Do()
        {
            if (_bodyInput.GetKey(_keyIndex) == GetInputState())
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}