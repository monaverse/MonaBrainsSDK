using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.Extensions.Interfaces
{ 
    public interface IVisualScriptInstructionTile : IInstructionTileWithPreload
    {
        string EventName { get; set; }
    }
}