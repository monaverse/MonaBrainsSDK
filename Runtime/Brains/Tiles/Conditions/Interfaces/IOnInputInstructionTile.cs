using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Enums;

namespace Mona.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnInputInstructionTile : IInstructionTileWithPreload
    {
        MonaInputType InputType { get; set; }
        MonaInputState InputState { get; set; }
    }
}
