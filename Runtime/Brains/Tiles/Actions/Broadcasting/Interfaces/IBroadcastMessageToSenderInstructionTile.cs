using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Broadcasting.Interfaces
{
    public interface IBroadcastMessageToSenderInstructionTile : IInstructionTileWithPreload
    {
        public string Message { get; set; }
    }
}