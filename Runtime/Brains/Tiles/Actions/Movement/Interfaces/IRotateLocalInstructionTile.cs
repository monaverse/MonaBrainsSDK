using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IRotateLocalInstructionTile : IInstructionTileWithPreload
    {
        float Angle { get; set; }
        MoveModeType Mode { get; set; }
        float Value { get; set; }
    }
}
