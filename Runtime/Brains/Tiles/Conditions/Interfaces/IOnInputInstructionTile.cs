using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Input.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnInputInstructionTile : IInstructionTileWithPreload
    {
        MonaInputType InputType { get; set; }
        MonaInputState InputState { get; set; }
    }
}
