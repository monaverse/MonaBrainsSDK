using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections.Generic;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using static Mona.SDK.Brains.Core.Brain.MonaBrainInput;
using Mona.SDK.Core.Input;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInteractInstructionTile : InputInstructionTile
    {
        public const string ID = "OnInteract";
        public const string NAME = "Interact";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnInteractInstructionTile);

        protected override MonaInputState GetInputState() => MonaInputState.Pressed;

        protected MonaInput _bodyInput;

        protected override void ProcessLocalInput()
        {
            if (BrainOnRemotePlayer()) return;

            var localInput = _brainInput.ProcessInput(_brain.LoggingEnabled, MonaInputType.Action, GetInputState());            
            if (localInput.GetButton(MonaInputType.Action) == GetInputState())
            {
                SetLocalInput(localInput);
            }
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
            if (_bodyInput.GetButton(MonaInputType.Action) == GetInputState())
            {
                return Complete(InstructionTileResult.Success);
            }         
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}