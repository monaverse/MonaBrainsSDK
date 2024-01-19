using Mona.SDK.Brains.Core.Enums;
using System;

namespace Mona.SDK.Brains.Core.Tiles
{
    public class InstructionTileCallback : IInstructionTileCallback
    {
        public Func<InstructionTileResult> Action { get; set; }
    }
}