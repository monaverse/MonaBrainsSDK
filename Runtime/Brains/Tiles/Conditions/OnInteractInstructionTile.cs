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
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Interfaces;

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

        protected List<IMonaLocalInput> _bodyInputs;

        protected override void ProcessLocalInput()
        {
            IMonaLocalInput _input = new MonaLocalInput();
            ProcessButton(_localInputs.Player.Action);
            
            if (_currentLocalInputState != MonaInputState.None && _currentLocalInputState == GetInputState())
            {
                _input.Type = MonaInputType.Action;
                _input.State = _currentLocalInputState;
                SetLocalInput(_input);
            }
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
                    if (input.State == GetInputState())
                    {
                        return Complete(InstructionTileResult.Success);
                    }
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}