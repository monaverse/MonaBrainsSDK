using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToSenderInstructionTile : IInstructionTileWithPreload
    {
        public string Message { get; set; }
    }
}