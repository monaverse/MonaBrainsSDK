using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RollLeftInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Roll Left";
        public const string NAME = "Roll Left";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(RollLeftInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.RollLeft;
    }
}