using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Input;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Mona.SDK.Brains.Core.Brain
{
    public class KeyState
    {
        public Key Key;
        public MonaInputState State;
        public bool Active;
        public List<IInputInstructionTile> Tiles = new List<IInputInstructionTile>();
    }

    public class MouseState
    {
        public Vector2 Axis = Vector2.zero;
        public Vector2 Scroll = Vector2.zero;
        public int ClickCount = 0;
        public MonaInputState LeftButtonState = MonaInputState.None;
        public MonaInputState RightButtonState = MonaInputState.None;
        public MonaInputState MiddleButtonState = MonaInputState.None;
        public MonaInputState BackButtonState = MonaInputState.None;
        public MonaInputState ForwardButtonState = MonaInputState.None;
    }

    public class MonaBrainInput : MonoBehaviour, IMonaBrainInput
    {
        private Inputs _inputs;
        public Inputs Inputs => _inputs;

        private Vector2 _externalMove;
        public bool _externalMoveSet = false;
        public Vector2 ExternalMove { 
            get => _externalMove; 
            set
            {
                _externalMoveSet = true;
                _externalMove = value;
            } 
        }

        private PlayerInput _playerInput;
        private MouseState _mouseState = new MouseState();
        private List<IInstructionTile> _activeListeners = new List<IInstructionTile>();
        private List<KeyState> _activeKeyListeners = new List<KeyState>();
        private IMonaBrainPlayer _player;

        public const float DEAD_ZONE = .1f;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();

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

        public void StartListening(IInstructionTile tile)
        {
            if (!_activeListeners.Contains(tile))
            {
                //Debug.Log($"{nameof(StartListening)} {tile} {tile.Brain.Body.ActiveTransform.name} {_activeListeners.Count}");
                _activeListeners.Add(tile);
            }
            UpdateActive();
        }

        public void StopListening(IInstructionTile tile)
        {
            if (_activeListeners.Contains(tile))
            {
                //Debug.Log($"{nameof(StopListening)} {tile} {tile.Brain.Body.ActiveTransform.name} {_activeListeners.Count}");
                _activeListeners.Remove(tile);
            }
            UpdateActive();
        }

        public int StartListeningForKey(Key key, IInputInstructionTile tile)
        {
            var idx = _activeKeyListeners.FindIndex(x => x.Key == key);
            if (idx == -1)
            {
                var listener = new KeyState() { Key = key, State = MonaInputState.None, Active = true };
                if(!listener.Tiles.Contains(tile))
                    listener.Tiles.Add(tile);
                _activeKeyListeners.Add(listener);
                idx = _activeKeyListeners.Count - 1;
            }
            else
            {
                var listener = _activeKeyListeners[idx];
                if(!listener.Tiles.Contains(tile))
                    listener.Tiles.Add(tile);
                listener.Active = true;
                _activeKeyListeners[idx] = listener;
            }

            return idx;
        }

        public void StopListeningForKey(Key key, IInputInstructionTile tile)
        {
            var idx = _activeKeyListeners.FindIndex(x => x.Key == key);
            if (idx > -1)
            {
                var listener = _activeKeyListeners[idx];
                if (listener.Tiles.Contains(tile))
                    listener.Tiles.Remove(tile);
                if (listener.Tiles.Count == 0)
                    listener.Active = false;
                _activeKeyListeners[idx] = listener;
            }
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

        public void EnableInput() => _inputs.Player.Enable();
        public void DisableInput() => _inputs.Player.Disable();

        private Dictionary<MonaInputType, MonaInputState> _buttons = new Dictionary<MonaInputType, MonaInputState>();
        private Vector2 _moveValue;
        private Vector2 _lookValue;
        private Ray _ray;

        private int _lastFrame = -1;
        private bool _wasTouching;
        private float _startTouchTime;
        private Vector2 _startTouchPosition;
        private Vector2 _lastTouchPosition;
        private Vector2 _mousePosition = Vector2.zero;

        public float GestureTimeout = 0f;
        public float JoystickSize = 500f;
        public float JoystickDeadZone = 100;

        public void SetTouchJoystickSettings(float gestureTimeout, float trueJoystickSize, float trueDeadZone)
        {
            GestureTimeout = gestureTimeout;
            JoystickSize = trueJoystickSize;
            JoystickDeadZone = trueDeadZone;
        }

        public MonaInput ProcessInput(bool logOutput, MonaInputType logType, MonaInputState logState = MonaInputState.Pressed)
        {
            if (_player == null) return default;
            if (!_inputs.Player.enabled) return default;

            if (_lastFrame != Time.frameCount)
            {
                _lastFrame = Time.frameCount;

                Vector2 value = Vector2.zero;
                if (Touch.activeTouches.Count > 0)
                {
                    if (!_wasTouching)
                    {
                        _wasTouching = true;
                        _startTouchTime = Time.time;
                        _startTouchPosition = Touch.activeTouches[0].screenPosition;                        
                    }
                    _lastTouchPosition = Touch.activeTouches[0].screenPosition;
                    value = _lastTouchPosition - _startTouchPosition;
                    value /= JoystickSize;

                    if (GestureTimeout == 0f)
                        ProcessAxis(MonaInputType.Move, value, JoystickDeadZone/JoystickSize);
                }
                else
                {
                    if (_wasTouching)
                    {
                        value = _lastTouchPosition - _startTouchPosition;
                        value /= JoystickSize;
                        if (GestureTimeout > 0)
                        {
                            if (Time.time - _startTouchTime < GestureTimeout)
                                ProcessAxis(MonaInputType.Move, value, JoystickDeadZone / JoystickSize);
                        }
                        else
                        {
                            ProcessAxis(MonaInputType.Move, value, JoystickDeadZone / JoystickSize);
                        }

                        if (value.magnitude < JoystickDeadZone / JoystickSize)
                        {
                            ProcessButton(MonaInputType.Action, true);
                        }
                        else
                        {
                            ProcessButton(MonaInputType.Action, _inputs.Player.Action);
                        }
                    }
                    else
                    {
                        ProcessAxis(MonaInputType.Move, _inputs.Player.Move);
                        ProcessButton(MonaInputType.Action, _inputs.Player.Action);
                    }
                }
                //Debug.Log($"move {JoystickDeadZone} {JoystickSize} {_moveValue} {_buttons[MonaInputType.Move]}");
                

                ProcessAxis(MonaInputType.Look, _inputs.Player.Look);
                ProcessButton(MonaInputType.Jump, _inputs.Player.Jump);
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

                if (Mouse.current != null)
                    _mousePosition = Mouse.current.position.ReadValue();

                if (Touch.activeTouches.Count > 0)
                {
                    _mousePosition = Touch.activeTouches[0].screenPosition;
                    //Debug.Log($"screen position {mouse} mouse: {Mouse.current.position.ReadValue()}");
                }
                else if (_wasTouching)
                {
                    _mousePosition = _lastTouchPosition;
                    _lastTouchPosition = Vector2.zero;                 
                    _wasTouching = false;
                }                

                //Debug.Log($"{nameof(ProcessInput)} {_mouse} {_buttons[MonaInputType.Action]}");

                if (_player.PlayerCamera != null)
                    _ray = _player.PlayerCamera.ScreenPointToRay(new Vector3(_mousePosition.x, _mousePosition.y, 0f));
                else
                    _ray = default;

                if (logOutput)
                {
                    if (logType == MonaInputType.Key)
                    {
                        for(var i = 0;i < _activeKeyListeners.Count; i++)
                        {
                            var listener = _activeKeyListeners[i];
                            //Debug.Log($"Key {listener.Key} {listener.State}");
                            //if (listener.State != MonaInputState.None)
                               // Debug.Log($"Key {listener.Key} {listener.State} {i}");
                        }
                    }
                    else
                    {
                        //if(_buttons[logType] == logState)
                        //    Debug.Log($"{logType} {_buttons[logType]}");
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
                MousePosition = _mousePosition,
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
            Keyboard keyboard;
            if (Keyboard.current == null)
            {
                keyboard = InputSystem.AddDevice<Keyboard>("Mobile");
                InputSystem.EnableDevice(keyboard);
            }
            else
                keyboard = Keyboard.current;

            var keyControl = keyboard[state.Key];
            
            if (keyControl.wasPressedThisFrame && state.State == MonaInputState.None)
                state.State = MonaInputState.Pressed;
            else if (keyControl.wasReleasedThisFrame && (state.State == MonaInputState.Pressed || state.State == MonaInputState.Held))
                state.State = MonaInputState.Up;
            else if (keyControl.isPressed)
                state.State = MonaInputState.Held;
            else
                state.State = MonaInputState.None;
        }

        public MouseState ProcessMouse()
        {
            Mouse mouse;
            if (Mouse.current == null)
            {
                mouse = InputSystem.AddDevice<Mouse>("Mobile");
                InputSystem.EnableDevice(mouse);
            }
            else
                mouse = Mouse.current;

            _mouseState.Axis = mouse.delta.value;
            _mouseState.Scroll = mouse.scroll.value;
            _mouseState.ClickCount = mouse.clickCount.value;

            if (mouse.leftButton.wasPressedThisFrame)
                _mouseState.LeftButtonState = MonaInputState.Pressed;
            else if (mouse.leftButton.wasReleasedThisFrame)
                _mouseState.LeftButtonState = MonaInputState.Up;
            else if (mouse.leftButton.isPressed)
                _mouseState.LeftButtonState = MonaInputState.Held;
            else
                _mouseState.LeftButtonState = MonaInputState.None;

            if (mouse.rightButton.wasPressedThisFrame)
                _mouseState.RightButtonState = MonaInputState.Pressed;
            else if (mouse.rightButton.wasReleasedThisFrame)
                _mouseState.RightButtonState = MonaInputState.Up;
            else if (mouse.rightButton.isPressed)
                _mouseState.RightButtonState = MonaInputState.Held;
            else
                _mouseState.RightButtonState = MonaInputState.None;

            if (mouse.middleButton.wasPressedThisFrame)
                _mouseState.MiddleButtonState = MonaInputState.Pressed;
            else if (mouse.middleButton.wasReleasedThisFrame)
                _mouseState.MiddleButtonState = MonaInputState.Up;
            else if (mouse.middleButton.isPressed)
                _mouseState.MiddleButtonState = MonaInputState.Held;
            else
                _mouseState.MiddleButtonState = MonaInputState.None;

            if (mouse.backButton.wasPressedThisFrame)
                _mouseState.BackButtonState = MonaInputState.Pressed;
            else if (mouse.backButton.wasReleasedThisFrame)
                _mouseState.BackButtonState = MonaInputState.Up;
            else if (mouse.backButton.isPressed)
                _mouseState.BackButtonState = MonaInputState.Held;
            else
                _mouseState.BackButtonState = MonaInputState.None;

            if (mouse.forwardButton.wasPressedThisFrame)
                _mouseState.ForwardButtonState = MonaInputState.Pressed;
            else if (mouse.forwardButton.wasReleasedThisFrame)
                _mouseState.ForwardButtonState = MonaInputState.Up;
            else if (mouse.forwardButton.isPressed)
                _mouseState.ForwardButtonState = MonaInputState.Held;
            else
                _mouseState.ForwardButtonState = MonaInputState.None;

            return _mouseState;
        }

        protected void ProcessAxis(MonaInputType type, Vector2 value, float deadZone)
        {
            if (type == MonaInputType.Move)
                _moveValue = value;
            else if (type == MonaInputType.Look)
                _lookValue = value;

            if (value.magnitude > deadZone)
                PerformInput(type);
            else
                ProcessAxis(MonaInputType.Move, _inputs.Player.Move);
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

        protected void ProcessButton(MonaInputType type, bool action)
        {
            if (action)
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