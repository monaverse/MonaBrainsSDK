using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Timing.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Timing
{
    [Serializable]
    public class WaitInstructionTile : InstructionTile, IWaitInstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "Wait";
        public const string NAME = "Wait";
        public const string CATEGORY = "Timing";
        public override Type TileType => typeof(WaitInstructionTile);

        [SerializeField]
        private float _seconds = 1f;

        [SerializeField] private string _secondsValueName;
        [BrainProperty] public float Seconds { get => _seconds; set => _seconds = value; }
        [BrainPropertyValueName("Seconds", typeof(IMonaVariablesFloatValue))] public string SecondsValueName { get => _secondsValueName; set => _secondsValueName = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyEvent> OnBodyEvent;

        private float _remaining;

        private bool _isRunning;

        private IMonaBrain _brain;
        private bool _active;

        public WaitInstructionTile()
        {
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

        public override void SetThenCallback(InstructionTileCallback thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback = thenCallback;
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback;
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
                if (!string.IsNullOrEmpty(_secondsValueName))
                    _seconds = _brain.Variables.GetFloat(_secondsValueName);
                _remaining = _seconds;
                _isRunning = true;
                //if(_brain.LoggingEnabled) Debug.Log($"{nameof(WaitInstructionTile)} remaining {_seconds}");
                AddFixedTickDelegate();
            }

            return Complete(InstructionTileResult.Running);
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            //OnBodyEvent = HandleBodyEvent;
            //MonaEventBus.Register<MonaBodyEvent>(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), OnBodyEvent);
        }

        /*
        private void HandleBodyEvent(MonaBodyEvent evt)
        {
            if (evt.Type == MonaBodyEventType.OnStop)
                LostControl();
        }*/

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            //MonaEventBus.Unregister(new EventHook(MonaBrainConstants.MONA_BRAINS_EVENT, _brain.Body), OnBodyEvent);
            //MonaEventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private void LostControl()
        {
            //if (_brain.LoggingEnabled) Debug.Log($"{nameof(WaitInstructionTile)} {nameof(LostControl)}");
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            if (!_brain.Body.HasControl())
            {
                LostControl();
                return;
            }

            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            _remaining -= deltaTime;
            //if (_brain.LoggingEnabled) Debug.Log($"{nameof(WaitInstructionTile)} {_remaining} of {_seconds}");
            if (_remaining <= 0)
            {
                _isRunning = false;
                Complete(InstructionTileResult.Success, true);
            }
        }

    }
}