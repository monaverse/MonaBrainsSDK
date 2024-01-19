using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Core.Tiles.ScriptableObjects
{
    public interface IInstructionTileDefinition
    {
        IInstructionTile Tile { get; }
        string Id { get; set; }
        string Name { get; set; }
        string Category { get; set; }
    }
}
