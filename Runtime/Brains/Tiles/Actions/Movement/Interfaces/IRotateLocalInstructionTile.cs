using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces
{
    public interface IRotateLocalInstructionTile : IInstructionTileWithPreload
    {
        float Angle { get; set; }
        MoveModeType Mode { get; set; }
        float Value { get; set; }
    }
}
