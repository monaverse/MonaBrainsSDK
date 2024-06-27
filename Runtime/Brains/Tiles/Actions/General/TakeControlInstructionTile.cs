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
    public class TakeControlInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "TakeControl";
        public const string NAME = "Change State";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(TakeControlInstructionTile);

        private IMonaBrain _brain;

        private Action<MonaStateAuthorityChangedEvent> OnStateAuthorityChanged;

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
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        private void HandleStateAuthorityChanged(MonaStateAuthorityChangedEvent evt)
        {
            RemoveEventDelegates();
            if(_brain.Body.HasControl())
                Complete(InstructionTileResult.Success, true);
            else
                Complete(InstructionTileResult.Failure, true);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl())
            {
                AddEventDelegates();
                _brain.Body.TakeControl();
                return Complete(InstructionTileResult.Running);
            }

            return Complete(InstructionTileResult.Success);
        }

        private void AddEventDelegates()
        {

            OnStateAuthorityChanged = HandleStateAuthorityChanged;
            MonaEventBus.Register<MonaStateAuthorityChangedEvent>(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _brain.Body), OnStateAuthorityChanged);
        }

        private void RemoveEventDelegates()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _brain.Body), OnStateAuthorityChanged);
        }
    }
}