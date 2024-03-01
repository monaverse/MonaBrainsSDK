﻿using Mona.SDK.Brains.Core.Enums;
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

        [BrainProperty]
        public float Seconds { get => _seconds; set => _seconds = value; }

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
            UpdateActive();
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
            if (!_active) return;

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }
        public override void Unload()
        {
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

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    //Debug.Log($"{nameof(WaitInstructionTile)} ThenCallback");
                    RemoveFixedTickDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
            if (!_isRunning)
            {
                _remaining = _seconds;
                _isRunning = true;
                AddFixedTickDelegate();
            }

            return Complete(InstructionTileResult.Running);
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            OnBodyEvent = HandleBodyEvent;
            EventBus.Register<MonaBodyEvent>(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), OnBodyEvent);
        }

        private void HandleBodyEvent(MonaBodyEvent evt)
        {
            if (evt.Type == MonaBodyEventType.OnStop)
                LostControl();
        }

        private void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.MONA_BRAINS_EVENT, _brain.Body), OnBodyEvent);
            //EventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private void LostControl()
        {
            Debug.Log($"{nameof(WaitInstructionTile)} {nameof(LostControl)}");
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
            if(_remaining <= 0)
            {
                _isRunning = false;
                Complete(InstructionTileResult.Success, true);
            }
        }

    }
}