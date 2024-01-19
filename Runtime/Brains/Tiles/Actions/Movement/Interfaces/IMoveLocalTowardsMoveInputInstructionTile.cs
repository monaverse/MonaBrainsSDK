using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IMoveLocalTowardsMoveInputInstructionTile : IInstructionTileWithPreload
    {
        MoveModeType Mode { get; set; }
        float Value { get; set; }
        bool ListenForTick { get; set; }

    }
}
