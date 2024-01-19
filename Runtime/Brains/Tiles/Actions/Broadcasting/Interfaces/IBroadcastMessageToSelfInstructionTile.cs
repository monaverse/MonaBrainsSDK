using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToSelfInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
    }
}