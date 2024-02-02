using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Input;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.Controls;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Interfaces;
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

        protected List<IMonaLocalInput> _bodyInputs;

        protected override void ProcessLocalInput()
        {
            IMonaLocalInput _input = new MonaLocalKeyInput();
            ProcessKey(Keyboard.current[_key]);
            if (_currentLocalInputState != MonaInputState.None && _currentLocalInputState == _inputState)
            {
                _input.Type = MonaInputType.Key;
                _input.State = _currentLocalInputState;
                _brain.Body.SetLocalInput(_input);
            }
        }

        private void ProcessKey(KeyControl keyControl)
        {
            if (keyControl.wasPressedThisFrame)
                _currentLocalInputState = MonaInputState.Pressed;
            else if (keyControl.wasReleasedThisFrame)
                _currentLocalInputState = MonaInputState.Up;
            else if (keyControl.isPressed)
                _currentLocalInputState = MonaInputState.Held;
            else
                _currentLocalInputState = MonaInputState.None;
        }

        protected override void HandleBodyInput(MonaInputEvent evt)
        {
            _bodyInputs = evt.Inputs;
        }

        public override InstructionTileResult Do()
        {
            if (_bodyInputs != null)
            {
                for (var i = 0; i < _bodyInputs.Count; i++)
                {
                    var input = _bodyInputs[i];
                    if (input is IMonaLocalKeyInput && input.Type == MonaInputType.Key && input.State == GetInputState())
                    {
                        return Complete(InstructionTileResult.Success);
                    }
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}