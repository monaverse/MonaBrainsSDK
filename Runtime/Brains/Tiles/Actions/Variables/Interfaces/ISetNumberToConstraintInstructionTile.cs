using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface ISetNumberToConstraintInstructionTile : IInstructionTileWithPreload
    {
        string NumberName { get; set; }
    }
}