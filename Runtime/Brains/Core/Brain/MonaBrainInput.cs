using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Input;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain
{
    public class KeyState
    {
        public Key Key;
        public MonaInputState State;
        public bool Active;
    }

    public class MonaBrainInput : MonoBehaviour, IMonaBrainInput
    {
        private Inputs _inputs;
        private PlayerInput _playerInput;
        private List<IMonaBody> _activeListeners = new List<IMonaBody>();
        private List<KeyState> _activeKeyListeners = new List<KeyState>();
        private IMonaBrainPlayer _player;

        public const float DEAD_ZONE = .1f;

        private void Awake()
        {
            _inputs = new Inputs();
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
                _playerInput = gameObject.AddComponent<PlayerInput>();

            _playerInput.onDeviceLost += OnDeviceLost;
            _playerInput.onDeviceRegained += OnDeviceRegained;
        }

        private void OnDestroy()
        {
            if (_playerInput != null)
            {
                _playerInput.onDeviceLost -= OnDeviceLost;
                _playerInput.onDeviceRegained -= OnDeviceRegained;
            }
        }

        public void SetPlayer(IMonaBrainPlayer player)
        {
            _player = player;
        }

        public void StartListening(IMonaBody body)
        {
            if (!_activeListeners.Contains(body))
                _activeListeners.Add(body);
            UpdateActive();
        }

        public void StopListening(IMonaBody body)
        {
            if (_activeListeners.Contains(body))
                _activeListeners.Remove(body);
            UpdateActive();
        }

        public int StartListeningForKey(Key key)
        {
            var idx = _activeKeyListeners.FindIndex(x => x.Key == key);
            if (idx == -1)
            {
                _activeKeyListeners.Add(new KeyState() { Key = key, State = MonaInputState.None, Active = true });
                idx = _activeKeyListeners.Count - 1;
            }
            else
                _activeKeyListeners[idx].Active = true;
            return idx;
        }

        public void StopListeningForKey(Key key)
        {
            var idx = _activeKeyListeners.FindIndex(x => x.Key == key);
            if (idx > -1)
                _activeKeyListeners[idx].Active = false;
        }

        private void UpdateActive()
        {
            if (_activeListeners.Count > 0)
            {
                _inputs.Player.Enable();
                _playerInput.ActivateInput();
            }
            else
            {
                _inputs.Player.Disable();
                _playerInput.DeactivateInput();
            }
        }

        private Dictionary<MonaInputType, MonaInputState> _buttons = new Dictionary<MonaInputType, MonaInputState>();
        private Vector2 _moveValue;
        private Vector2 _lookValue;
        private Ray _ray;

        private int _lastFrame;

        public MonaInput ProcessInput(bool logOutput, MonaInputType logType, MonaInputState logState = MonaInputState.Pressed)
        {
            if (_player == null) return default;

            if (_lastFrame != Time.frameCount)
            {
                _lastFrame = Time.frameCount;

                ProcessAxis(MonaInputType.Move, _inputs.Player.Move);
                ProcessAxis(MonaInputType.Look, _inputs.Player.Look);
                ProcessButton(MonaInputType.Jump, _inputs.Player.Jump);
                ProcessButton(MonaInputType.Action, _inputs.Player.Action);
                ProcessButton(MonaInputType.Sprint, _inputs.Player.Sprint);
                ProcessButton(MonaInputType.SwitchCamera, _inputs.Player.SwitchCamera);
                ProcessButton(MonaInputType.Respawn, _inputs.Player.Respawn);
                ProcessButton(MonaInputType.Debug, _inputs.Player.Debug);
                ProcessButton(MonaInputType.ToggleUI, _inputs.Player.ToggleUI);
                ProcessButton(MonaInputType.EmoteWheel, _inputs.Player.EmoteWheel);
                ProcessButton(MonaInputType.EmojiTray, _inputs.Player.EmojiTray);
                ProcessButton(MonaInputType.ToggleNametags, _inputs.Player.ToggleNametags);
                ProcessButton(MonaInputType.Escape, _inputs.Player.Escape);
                ProcessButton(MonaInputType.OpenChat, _inputs.Player.OpenChat);
                ProcessButton(MonaInputType.ToggleMouseCapture, _inputs.Player.ToggleMouseCapture);

                for (var i = 0; i < _activeKeyListeners.Count; i++)
                {
                    if (_activeKeyListeners[i].Active)
                        ProcessKey(i, _activeKeyListeners[i]);
                    else
                        _activeKeyListeners[i].State = MonaInputState.None;
                }

                var mouse = Mouse.current.position.ReadValue();
                if (_player.PlayerCamera != null)
                    _ray = _player.PlayerCamera.Transform.GetComponent<Camera>().ScreenPointToRay(new Vector3(mouse.x, mouse.y, 0f));
                else
                    _ray = default;

                if (logOutput)
                {
                    if (logType == MonaInputType.Key)
                    {
                        for(var i = 0;i < _activeKeyListeners.Count; i++)
                        {
                            var listener = _activeKeyListeners[i];
                            if (listener.State != MonaInputState.None)
                                Debug.Log($"Key {listener.Key} {listener.State}");
                        }
                    }
                    else
                    {
                        if(_buttons[logType] == logState)
                            Debug.Log($"{logType} {_buttons[logType]}");
                    }
                }

            }

            return new MonaInput()
            {
                PlayerId = _player.PlayerId,

                Move = _buttons[MonaInputType.Move],
                Look = _buttons[MonaInputType.Look],
                Jump = _buttons[MonaInputType.Jump],
                Action = _buttons[MonaInputType.Action],
                Sprint = _buttons[MonaInputType.Sprint],
                SwitchCamera = _buttons[MonaInputType.SwitchCamera],
                Respawn = _buttons[MonaInputType.Respawn],
                Debug = _buttons[MonaInputType.Debug],
                ToggleUI = _buttons[MonaInputType.ToggleUI],
                EmoteWheel = _buttons[MonaInputType.EmoteWheel],
                EmojiTray = _buttons[MonaInputType.EmojiTray],
                ToggleNametags = _buttons[MonaInputType.ToggleNametags],
                Escape = _buttons[MonaInputType.Escape],
                OpenChat = _buttons[MonaInputType.OpenChat],
                ToggleMouseCapture = _buttons[MonaInputType.ToggleMouseCapture],

                Key0 = GetKey(0),
                Key1 = GetKey(1),
                Key2 = GetKey(2),
                Key3 = GetKey(3),
                Key4 = GetKey(4),
                Key5 = GetKey(5),
                Key6 = GetKey(6),
                Key7 = GetKey(7),
                Key8 = GetKey(8),
                Key9 = GetKey(9),
                Key10 = GetKey(10),

                MoveValue = _moveValue,
                LookValue = _lookValue,
                Origin = Vector3.zero,
                Direction = Vector3.zero,
                Ray = _ray
            };
        }

        public MonaInputState GetKey(int idx)
        {
            if (idx > _activeKeyListeners.Count-1) return MonaInputState.None;
            return _activeKeyListeners[idx].State;
        }

        private void ProcessKey(int index, KeyState state)
        {
            var keyControl = Keyboard.current[state.Key];
            if (keyControl.wasPressedThisFrame)
                state.State = MonaInputState.Pressed;
            else if (keyControl.wasReleasedThisFrame)
                state.State = MonaInputState.Up;
            else if (keyControl.isPressed)
                state.State = MonaInputState.Held;
            else
                state.State = MonaInputState.None;
        }

        protected void ProcessAxis(MonaInputType type, InputAction action)
        {
            var value = action.ReadValue<Vector2>();

            if (type == MonaInputType.Move)
                _moveValue = value;
            else if (type == MonaInputType.Look)
                _lookValue = value;

            if (value.magnitude > DEAD_ZONE)
                PerformInput(type);
            else
                ReleaseInput(type);
        }

        protected void ProcessButton(MonaInputType type, InputAction action)
        {
            if (action.IsPressed())
                PerformInput(type);
            else
                ReleaseInput(type);
        }

        private void PerformInput(MonaInputType type)
        {
            if (!_buttons.ContainsKey(type))
                _buttons.Add(type, MonaInputState.None);

            if (_buttons[type] <= MonaInputState.Up)
            {
                _buttons[type] = MonaInputState.Pressed;
            }
            else if (_buttons[type] == MonaInputState.Pressed)
            {
                _buttons[type] = MonaInputState.Held;
            }
        }

        private void ReleaseInput(MonaInputType type)
        {
            if (!_buttons.ContainsKey(type))
                _buttons.Add(type, MonaInputState.None);

            if (_buttons[type] > MonaInputState.Up)
            {
                _buttons[type] = MonaInputState.Up;
            }
            else if (_buttons[type] == MonaInputState.Up)
            {
                _buttons[type] = MonaInputState.None;
            }
        }

        private void OnDeviceLost(PlayerInput obj)
        {
            Debug.Log("Input Device Lost");
        }

        private void OnDeviceRegained(PlayerInput obj)
        {
            Debug.Log("Input Device Regained");
        }

    }
}