using Mona.Brains.Core;
using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Events;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Enums;
using Mona.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInputInstructionTile : InstructionTile, IOnInputInstructionTile, IDisposable, IConditionInstructionTile, IStartableInstructionTile, IInputInstructionTile
    {
        public const string ID = "OnKeyDown";
        public const string NAME = "On Input";
        public const string CATEGORY = "Condition/Input";
        public override Type TileType => typeof(OnInputInstructionTile);

        [SerializeField]
        private MonaInputType _inputType;
        [BrainPropertyEnum(true)]
        public MonaInputType InputType { get => _inputType; set => _inputType = value; }

        [SerializeField]
        private MonaInputState _inputState = MonaInputState.None;
        [BrainPropertyEnum(false)]
        public MonaInputState InputState { get => _inputState; set => _inputState = value; }

        public const float DEAD_ZONE = .1f;

        private IMonaBrain _brain;

        private PlayerInput _inputManager;
        private Inputs _inputs;
        private Action<MonaTileTickEvent> OnTileTick;
        private bool _inputUsingGamepad;
        private bool _wasTicking;

        private float _mouseLookSensitivity = 30f;

        private MonaInputState _currentInputState;

        public OnInputInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            ConfigureInput();

            OnTileTick = HandleTileTick;
            EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);

        }

        private Vector2 _movementDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
            set => _brain.State.Set(MonaBrainConstants.RESULT_MOVE_DIRECTION, (Vector2)value);
        }

        private Vector2 _mouseDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOUSE_DIRECTION);
            set => _brain.State.Set(MonaBrainConstants.RESULT_MOUSE_DIRECTION, (Vector3)value);
        }

        public void Dispose()
        {
            if (_inputManager != null)
            {
                _inputManager.onDeviceLost -= OnDeviceLost;
                _inputManager.onDeviceRegained -= OnDeviceRegained;
            }
            EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);
        }

        private void ConfigureInput()
        {
            _inputManager = _brain.GameObject.GetComponent<PlayerInput>();
            if (_inputManager == null)
                _inputManager = _brain.GameObject.AddComponent<PlayerInput>();

            _inputManager.onDeviceLost += OnDeviceLost;
            _inputManager.onDeviceRegained += OnDeviceRegained;

            _inputs = new Inputs();
            _inputs.Player.Enable();

            _inputManager.ActivateInput();

            RegisterActionChange();
        }

        private void ProcessInput(MonaInputType inputType)
        {
            switch(inputType)
            {
                case MonaInputType.Move:
                    ProcessAxis(_inputs.Player.Move);
                    _movementDirection = _inputs.Player.Move.ReadValue<Vector2>();
                    break;
                case MonaInputType.Look:
                    ProcessAxis(_inputs.Player.Look);
                    _mouseDirection = _inputs.Player.Look.ReadValue<Vector2>() * (Cursor.visible ? 0 : _mouseLookSensitivity);
                    break;
                case MonaInputType.Jump: ProcessButton(_inputs.Player.Jump); break;
                case MonaInputType.Action: ProcessButton(_inputs.Player.Action); break;
                case MonaInputType.Sprint: ProcessButton(_inputs.Player.Sprint); break;
                case MonaInputType.Interact: ProcessButton(_inputs.Player.Interact); break;
                case MonaInputType.SwitchCamera: ProcessButton(_inputs.Player.SwitchCamera); break;
                case MonaInputType.Respawn: ProcessButton(_inputs.Player.Respawn); break;
                case MonaInputType.Debug: ProcessButton(_inputs.Player.Debug); break;
                case MonaInputType.ToggleUI: ProcessButton(_inputs.Player.ToggleUI); break;
                case MonaInputType.EmoteWheel: ProcessButton(_inputs.Player.EmoteWheel); break;
                case MonaInputType.EmojiTray: ProcessButton(_inputs.Player.EmojiTray); break;
                case MonaInputType.ToggleNametags: ProcessButton(_inputs.Player.ToggleNametags); break;
                case MonaInputType.Escape: ProcessButton(_inputs.Player.Escape); break;
                case MonaInputType.OpenChat: ProcessButton(_inputs.Player.OpenChat); break;
                case MonaInputType.ToggleMouseCapture: ProcessButton(_inputs.Player.ToggleMouseCapture); break;
            }

            if (_currentInputState != MonaInputState.None && _currentInputState == _inputState)
            {
                _wasTicking = true;
                EventBus.Trigger(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, _brain), new MonaHasInputTickEvent(Time.frameCount));
            }
        }

        private void ProcessButton(InputAction action)
        {
            if (action.IsPressed())
                PerformInput();
            else
                ReleaseInput();
        }

        private void ProcessAxis(InputAction action)
        {
            var value = action.ReadValue<Vector2>();
            if (value.magnitude > DEAD_ZONE)
                PerformInput();
            else
                ReleaseInput();
        }

        private void PerformInput()
        {
            if (_currentInputState <= MonaInputState.Up)
            {
                _currentInputState = MonaInputState.Pressed;
                //Debug.Log($"{nameof(PerformInput)} {t} {_monaInputs[t]}");
            }
            else if (_currentInputState == MonaInputState.Pressed)
            {
                _currentInputState = MonaInputState.Held;
                //Debug.Log($"{nameof(PerformInput)} {t} {_monaInputs[t]}");
            }
        }

        private void ReleaseInput()
        {
            if (_currentInputState > MonaInputState.Up)
            {
                _currentInputState = MonaInputState.Up;
                //Debug.Log($"{nameof(ReleaseInput)} {t} {_monaInputs[t]}");
            }
            else if (_currentInputState == MonaInputState.Up)
            {
                _currentInputState = MonaInputState.None;
            }
        }

        private void HandleTileTick(MonaTileTickEvent evt)
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

        private void RegisterActionChange()
        {
            InputSystem.onActionChange += (obj, change) =>
            {
                if (change == InputActionChange.ActionPerformed)
                {
                    var inputAction = (InputAction)obj;
                    var lastControl = inputAction.activeControl;
                    var lastDevice = lastControl.device;

                    OnControlSchemeChanged(lastDevice.displayName);
                }
            };
        }

        public void OnControlSchemeChanged(string device)
        {
            if (device == "Mouse" || device == "Keyboard")
            {
                _inputUsingGamepad = false;
            }
            else
            {
                _inputUsingGamepad = true;
            }
        }

        public override InstructionTileResult Do()
        {
            if (_inputState == _currentInputState)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}