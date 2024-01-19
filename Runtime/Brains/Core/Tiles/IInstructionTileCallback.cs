using Mona.SDK.Brains.Core.Enums;
using System;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IInstructionTileCallback
    {
        Func<InstructionTileResult> Action { get; set; }
    }
}