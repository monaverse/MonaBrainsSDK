using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;

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

        protected override MonaInputState GetInputState() => _inputState;

        private float _mouseLookSensitivity = 30f;

        protected MonaInput _bodyInput;

        protected override void ProcessLocalInput()
        {
            if (BrainOnRemotePlayer()) return;

            var localInput = _brainInput.ProcessInput(_brain.LoggingEnabled, _inputType, GetInputState());
            if (localInput.GetButton(_inputType) == _inputState)
            {
                Debug.Log($"{nameof(OnInputInstructionTile)} {_inputType} {_inputState}");
                SetLocalInput(localInput);
            }
        }

        protected override void HandleBodyInput(MonaInputEvent evt)
        {
            //Debug.Log($"{nameof(OnInputInstructionTile)} {_inputType} {evt.Input.GetButton(_inputType)}");
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
            if (_bodyInput.GetButton(_inputType) == _inputState)
            {
                Debug.Log($"{nameof(OnInputInstructionTile)} DO {_inputType} {_inputState}");
                //if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnInputInstructionTile)}.{nameof(Do)} input active {_inputType} {_inputState}");
                return Complete(InstructionTileResult.Success);
            }            
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}