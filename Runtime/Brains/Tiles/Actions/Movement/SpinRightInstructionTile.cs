using Mona.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinRightInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Right";
        public const string NAME = "Spin Right";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(SpinRightInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinRight;
    }
}