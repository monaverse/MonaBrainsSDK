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
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Interfaces;

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
        [BrainPropertyEnum(false)]
        public MonaInputState InputState { get => _inputState; set => _inputState = value; }

        protected override MonaInputState GetInputState() => _inputState;

        private float _mouseLookSensitivity = 30f;

        protected List<IMonaLocalInput> _bodyInputs;

        protected override void ProcessLocalInput()
        {
            ProcessInput(_inputType);
        }

        void ProcessInput(MonaInputType inputType)
        {
            IMonaLocalInput input = new MonaLocalInput();
            switch(inputType)
            {
                case MonaInputType.Move:
                    input = new MonaLocalAxisInput();
                    ProcessAxis(_localInputs.Player.Move);
                    ((IMonaLocalAxisInput)input).Value = _localInputs.Player.Move.ReadValue<Vector2>();
                    break;
                case MonaInputType.Look:
                    input = new MonaLocalAxisInput();
                    ProcessAxis(_localInputs.Player.Look);
                    ((IMonaLocalAxisInput)input).Value = _localInputs.Player.Look.ReadValue<Vector2>() * (Cursor.visible ? 0 : _mouseLookSensitivity);
                    break;
                case MonaInputType.Jump: ProcessButton(_localInputs.Player.Jump); break;
                case MonaInputType.Action: ProcessButton(_localInputs.Player.Action); break;
                case MonaInputType.Sprint: ProcessButton(_localInputs.Player.Sprint); break;
                case MonaInputType.Interact: ProcessButton(_localInputs.Player.Interact); break;
                case MonaInputType.SwitchCamera: ProcessButton(_localInputs.Player.SwitchCamera); break;
                case MonaInputType.Respawn: ProcessButton(_localInputs.Player.Respawn); break;
                case MonaInputType.Debug: ProcessButton(_localInputs.Player.Debug); break;
                case MonaInputType.ToggleUI: ProcessButton(_localInputs.Player.ToggleUI); break;
                case MonaInputType.EmoteWheel: ProcessButton(_localInputs.Player.EmoteWheel); break;
                case MonaInputType.EmojiTray: ProcessButton(_localInputs.Player.EmojiTray); break;
                case MonaInputType.ToggleNametags: ProcessButton(_localInputs.Player.ToggleNametags); break;
                case MonaInputType.Escape: ProcessButton(_localInputs.Player.Escape); break;
                case MonaInputType.OpenChat: ProcessButton(_localInputs.Player.OpenChat); break;
                case MonaInputType.ToggleMouseCapture: ProcessButton(_localInputs.Player.ToggleMouseCapture); break;
            }

            if (_currentLocalInputState != MonaInputState.None && _currentLocalInputState == _inputState)
            {
                input.Type = inputType;
                input.State = _currentLocalInputState;
                _brain.Body.SetLocalInput(input);
            }
        }

        private void HandleTick(MonaTickEvent evt)
        {
            if((_brain.Body == null || _brain.Body.HasControl()) && _inputType != MonaInputType.None)
                ProcessInput(_inputType);
        }

        void OnDeviceLost(PlayerInput obj)
        {
            Debug.Log("Input Device Lost");
        }

        void OnDeviceRegained(PlayerInput obj)
        {
            Debug.Log("Input Device Regained");
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
                    if (input.Type == _inputType && input.State == _inputState)
                    {
                        if (_brain.LoggingEnabled)
                            Debug.Log($"{nameof(OnInputInstructionTile)}.{nameof(Do)} input active {_inputType} {_inputState}");
                        return Complete(InstructionTileResult.Success);
                    }
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}