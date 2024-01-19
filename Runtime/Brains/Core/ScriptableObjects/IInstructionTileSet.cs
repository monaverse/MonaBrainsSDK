using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using System.Collections.Generic;

namespace Mona.Brains.Core.ScriptableObjects
{
    public interface IInstructionTileSet
    {
        string Version { get; }
        List<IInstructionTileDefinition> ConditionTiles { get; }
        List<IInstructionTileDefinition> ActionTiles { get; }
        IInstructionTileDefinition Find(string id);
    }
}
