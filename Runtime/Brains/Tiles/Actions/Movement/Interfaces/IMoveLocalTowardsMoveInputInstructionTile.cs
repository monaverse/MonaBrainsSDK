using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IMoveLocalTowardsMoveInputInstructionTile : IInstructionTileWithPreload
    {
        float Value { get; set; }
    }
}
