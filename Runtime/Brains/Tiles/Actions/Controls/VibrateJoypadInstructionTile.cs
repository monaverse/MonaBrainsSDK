using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Tiles.Actions.Timing.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Timing
{
    [Serializable]
    public class VibrateJoypadInstructionTile : InstructionTile, IWaitInstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "VibrateJoypad";
        public const string NAME = "Vibrate Joypad";
        public const string CATEGORY = "Controls";
        public override Type TileType => typeof(VibrateJoypadInstructionTile);

        [SerializeField] private VibrationDeviceType _device = VibrationDeviceType.Joypad;
        [BrainPropertyEnum(true)] public VibrationDeviceType Device { get => _device; set => _device = value; }

        [SerializeField] private float _strength = 0.5f;
        [SerializeField] private string _strengthName;
        [BrainPropertyShow(nameof(Device), (int)VibrationDeviceType.Joypad)]
        [BrainProperty] public float Strength { get => _strength; set => _strength = value; }
        [BrainPropertyValueName("Strength", typeof(IMonaVariablesFloatValue))] public string StrengthName { get => _strengthName; set => _strengthName = value; }

        [SerializeField] private float _seconds = 0.2f;
        [SerializeField] private string _secondsValueName;
        [BrainPropertyShow(nameof(Device), (int)VibrationDeviceType.Joypad)]
        [BrainProperty] public float Seconds { get => _seconds; set => _seconds = value; }
        [BrainPropertyValueName("Seconds", typeof(IMonaVariablesFloatValue))] public string SecondsValueName { get => _secondsValueName; set => _secondsValueName = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyEvent> OnBodyEvent;

        private float _remaining;
        private bool _isRunning;

        private IMonaBrain _brain;
        private bool _active;
        private bool _hadControl;

        public VibrateJoypadInstructionTile() { }

        public enum VibrationDeviceType
        {
            Joypad,
            PhoneOrTablet
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_isRunning)
                    LostControl();

                return;
            }

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }
        public override void Unload(bool destroy = false)
        {
            SetActive(false);
            _isRunning = false;
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _isRunning;
        }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public override InstructionTileResult Do()
        {
            if (!_isRunning)
            {
                if (!string.IsNullOrEmpty(_strengthName))
                    _strength = _brain.Variables.GetFloat(_strengthName);

                if (!string.IsNullOrEmpty(_secondsValueName))
                    _seconds = _brain.Variables.GetFloat(_secondsValueName);

                _remaining = _seconds;
                
                

                if (_device == VibrationDeviceType.PhoneOrTablet)
                {
                    if (Application.isMobilePlatform)
                        Handheld.Vibrate();

                    return Complete(InstructionTileResult.Success);
                }
                else if (_device == VibrationDeviceType.Joypad)
                {
                    _isRunning = true;
                    _hadControl = _brain.Body.HasControl();
                    SetControllerVibration(_strength);
                    AddFixedTickDelegate();
                }
                else
                {
                    return Complete(InstructionTileResult.Success);
                }
            }

            return Complete(InstructionTileResult.Running);
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            _remaining -= deltaTime;

            if (_remaining <= 0)
            {
                _isRunning = false;
                SetControllerVibration(0f);
                Complete(InstructionTileResult.Success, true);
            }
        }

        private void SetControllerVibration(float strength)
        {
            UnityEngine.InputSystem.Gamepad.current.SetMotorSpeeds(strength, strength);
        }

    }
}