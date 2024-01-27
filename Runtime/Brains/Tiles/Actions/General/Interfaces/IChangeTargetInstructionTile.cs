using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeTargetInstructionTile : IInstructionTileWithPreload
    {
        string Target { get; set; }
    }
}