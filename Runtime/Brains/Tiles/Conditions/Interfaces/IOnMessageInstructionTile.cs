using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnMessageInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
    }
}