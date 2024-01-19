using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeStateInstructionTile : IInstructionTileWithPreload
    {
        string State { get; set; }
    }
}