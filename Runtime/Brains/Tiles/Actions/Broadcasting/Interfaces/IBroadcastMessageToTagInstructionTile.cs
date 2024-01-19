using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToTagInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
        string Tag { get; set; }
    }
}