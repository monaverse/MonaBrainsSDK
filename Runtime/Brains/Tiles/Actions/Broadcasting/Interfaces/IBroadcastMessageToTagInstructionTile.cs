using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToTagInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
        string Tag { get; set; }
    }
}