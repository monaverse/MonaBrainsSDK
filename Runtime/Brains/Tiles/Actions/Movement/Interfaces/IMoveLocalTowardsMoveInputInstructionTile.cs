using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IMoveLocalTowardsMoveInputInstructionTile : IInstructionTileWithPreload
    {
        MoveModeType Mode { get; set; }
        float Value { get; set; }
        bool ListenForTick { get; set; }

    }
}
