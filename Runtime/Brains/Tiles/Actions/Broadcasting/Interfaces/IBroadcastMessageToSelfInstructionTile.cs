using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToSelfInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
    }
}