using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Extensions.Interfaces
{ 
    public interface IVisualScriptInstructionTile : IInstructionTileWithPreload
    {
        string EventName { get; set; }
    }
}