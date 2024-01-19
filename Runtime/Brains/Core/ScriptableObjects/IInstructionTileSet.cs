using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    public interface IInstructionTileSet
    {
        string Version { get; }
        List<IInstructionTileDefinition> ConditionTiles { get; }
        List<IInstructionTileDefinition> ActionTiles { get; }
        IInstructionTileDefinition Find(string id);
    }
}
