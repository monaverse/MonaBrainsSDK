using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeColorInstructionTile : IInstructionTileWithPreloadAndPageAndInstruction
    {
        Color Color { get; set; }
        EasingType Easing { get; set; }
        float Duration { get; set; }
    }
}
