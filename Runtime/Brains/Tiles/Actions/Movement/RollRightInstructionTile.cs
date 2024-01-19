using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RollRightInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Roll Right";
        public const string NAME = "Roll Right";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(RollRightInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.RollRight;
    }
}