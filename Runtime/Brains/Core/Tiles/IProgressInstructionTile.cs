using Mona.SDK.Brains.Core.Enums;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IProgressInstructionTile : IInstructionTile
    {
        public bool InProgress { get; }
        public InstructionTileResult Continue();
    }
}
