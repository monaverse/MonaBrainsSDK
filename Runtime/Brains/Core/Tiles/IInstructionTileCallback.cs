using Mona.Brains.Core.Enums;
using System;

namespace Mona.Brains.Core.Tiles
{
    public interface IInstructionTileCallback
    {
        Func<InstructionTileResult> Action { get; set; }
    }
}