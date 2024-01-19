using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Timing.Interfaces
{ 
    public interface IWaitInstructionTile : IInstructionTile
    {
        float Seconds { get; set; }
    }
}