using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using System;

namespace Mona.SDK.Brains.Core.Tiles
{
    public struct InstructionTileCallback
    {
        public IInstructionTile Tile { get; set; }
        public Func<InstructionTileCallback, InstructionTileResult> ActionCallback { get; set; }
    }
}