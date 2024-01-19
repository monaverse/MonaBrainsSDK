using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IMoveLocalInstructionTile : IInstructionTileWithPreload
    {
        float Distance { get; set; }
        EasingType Easing { get; set; }
        MoveModeType Mode { get; set; }
        float Value { get; set; }
    }
}
