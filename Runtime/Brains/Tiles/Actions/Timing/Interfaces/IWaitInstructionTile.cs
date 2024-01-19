using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Timing.Interfaces
{ 
    public interface IWaitInstructionTile : IInstructionTile
    {
        float Seconds { get; set; }
    }
}