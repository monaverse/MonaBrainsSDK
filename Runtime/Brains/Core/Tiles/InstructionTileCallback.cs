using Mona.Brains.Core.Enums;
using System;

namespace Mona.Brains.Core.Tiles
{
    public class InstructionTileCallback : IInstructionTileCallback
    {
        public Func<InstructionTileResult> Action { get; set; }
    }
}