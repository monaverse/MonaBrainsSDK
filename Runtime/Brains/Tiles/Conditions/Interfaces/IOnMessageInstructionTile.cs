using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnMessageInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
    }
}