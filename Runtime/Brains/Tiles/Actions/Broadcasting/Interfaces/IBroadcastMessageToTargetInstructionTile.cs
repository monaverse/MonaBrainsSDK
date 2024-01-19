using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToTargetInstructionTile : IInstructionTileWithPreload
    {
        string Message { get; set; }
        string TargetValue { get; set; }
    }
}