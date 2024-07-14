using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Unity.Profiling;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Utils;
using Unity.VisualScripting;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class TakeControlInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, ITakeAuthorityInstructionTile
    {
        public const string ID = "TakeControl";
        public const string NAME = "Change State";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(TakeControlInstructionTile);

        private IMonaBrain _brain;
        private float _timeout;

        private Action<MonaStateAuthorityChangedEvent> OnStateAuthorityChanged;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(TakeControlInstructionTile)}.{nameof(Do)}");

        public TakeControlInstructionTile() { }


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
            RemoveEventDelegates();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        private void HandleStateAuthorityChanged(MonaStateAuthorityChangedEvent evt)
        {
            Debug.Log($"{nameof(TakeControlInstructionTile)} {nameof(HandleStateAuthorityChanged)} owned? {evt.Owned}", _brain.Body.Transform.gameObject);
            if (evt.Owned)
            {
                RemoveEventDelegates();
                Complete(InstructionTileResult.Success, true);
            }
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            _timeout -= evt.DeltaTime;
            if (_timeout < 0)
            {
                Debug.Log($"{nameof(TakeControlInstructionTile)} request timedout", _brain.Body.Transform.gameObject);
                RemoveEventDelegates();
                Complete(_brain.Body.HasControl() ? InstructionTileResult.Success : InstructionTileResult.Failure, true);
            }
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl())
            {
                Debug.Log($"{nameof(TakeControlInstructionTile)} {nameof(Do)}", _brain.Body.Transform.gameObject);                
                _timeout = 1f;
                _brain.Body.TakeControl();
                if (_brain.Body.HasControl() && _brain.Variables.HasControl())
                    return Complete(InstructionTileResult.Success);
                else
                {
                    AddEventDelegates();
                    return Complete(InstructionTileResult.Running);
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private void AddEventDelegates()
        {
            //Debug.Log($"{nameof(TakeControlInstructionTile)} {nameof(AddEventDelegates)}", _brain.Body.Transform.gameObject);
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            OnStateAuthorityChanged = HandleStateAuthorityChanged;
            MonaEventBus.Register<MonaStateAuthorityChangedEvent>(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _brain.Body), OnStateAuthorityChanged);
        }

        private void RemoveEventDelegates()
        {
            //Debug.Log($"{nameof(TakeControlInstructionTile)} {nameof(RemoveEventDelegates)}", _brain.Body.Transform.gameObject);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _brain.Body), OnStateAuthorityChanged);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }
    }
}