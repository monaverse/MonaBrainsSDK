using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToTargetInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
        string TargetValue { get; set; }
    }
}