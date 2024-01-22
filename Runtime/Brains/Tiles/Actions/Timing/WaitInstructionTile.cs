using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Timing.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Timing
{
    [Serializable]
    public class WaitInstructionTile : InstructionTile, IWaitInstructionTile, IActionInstructionTile
    {
        public const string ID = "Wait";
        public const string NAME = "Wait";
        public const string CATEGORY = "Timing";
        public override Type TileType => typeof(WaitInstructionTile);

        [SerializeField]
        private float _seconds = 1f;

        [BrainProperty]
        public float Seconds { get => _seconds; set => _seconds = value; }

        private Action<MonaTileTickEvent> OnTick;

        private float _remaining;

        private bool _isRunning;

        public WaitInstructionTile()
        {
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    //Debug.Log($"{nameof(WaitInstructionTile)} ThenCallback");
                    EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
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
                OnTick = HandleTick;
                EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
            }

            return Complete(InstructionTileResult.Running);
        }

        private void HandleTick(MonaTileTickEvent evt)
        {
            Tick(evt.DeltaTime);
        }

        private void Tick(float deltaTime)
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