using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Extensions.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Extensions
{
    [Serializable]
    public class VisualScriptingInstructionTile : InstructionTile, IVisualScriptInstructionTile, IActionInstructionTile
    {
        public const string ID = "VisualScript";
        public const string NAME = "Visual Script";
        public const string CATEGORY = "Extensions";
        public override Type TileType => typeof(VisualScriptingInstructionTile);

        [SerializeField]
        private string _eventName;

        [BrainProperty]
        public string EventName { get => _eventName; set => _eventName = value; }

        private Action<MonaBrainsThenEvent> OnVisualScriptReceive;

        private IMonaBrain _brain;

        private bool _isRunning;

        public VisualScriptingInstructionTile()
        {
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
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
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.MONA_BRAINS_THEN_EVENT, _brain), OnVisualScriptReceive);
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }


        public override InstructionTileResult Do()
        {
            TriggerOnExecute(InstructionTileResult.Running, null);
            if (!_isRunning)
            {
                _isRunning = true;
                
                OnVisualScriptReceive = HandleVisualScriptReceive;
                MonaEventBus.Register<MonaBrainsThenEvent>(new EventHook(MonaBrainConstants.MONA_BRAINS_THEN_EVENT, _brain), OnVisualScriptReceive);
                MonaEventBus.Trigger<MonaBrainsDoEvent>(new EventHook(MonaBrainConstants.MONA_BRAINS_DO_EVENT), new MonaBrainsDoEvent(_brain, _eventName));
            }

            Debug.Log($"{nameof(VisualScriptingInstructionTile)} Do");

            //if result from visual script came back same frame before the end of this method.
            if (LastResult == InstructionTileResult.Success) return InstructionTileResult.Success;

            return Complete(InstructionTileResult.Running);
        }

        private void HandleVisualScriptReceive(MonaBrainsThenEvent evt)
        {
            if (evt.EventName == _eventName)
            {
                Complete(InstructionTileResult.Success, true);
                _isRunning = false;
            }
        }
    }
}