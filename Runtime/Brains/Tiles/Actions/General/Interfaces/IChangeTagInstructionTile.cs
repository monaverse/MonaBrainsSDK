using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeTagInstructionTile : IInstructionTileWithPreload
    {
        string Tag { get; set; }
    }
}