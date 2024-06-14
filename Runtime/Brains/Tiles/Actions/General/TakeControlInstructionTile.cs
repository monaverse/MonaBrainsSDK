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


        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(TakeControlInstructionTile)}.{nameof(Do)}");

        public TakeControlInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl())
                _brain.Body.TakeControl();

            return Complete(InstructionTileResult.Success);
        }
    }
}