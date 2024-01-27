using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinRightInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Right";
        public const string NAME = "Spin Right";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(SpinRightInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinRight;
    }
}