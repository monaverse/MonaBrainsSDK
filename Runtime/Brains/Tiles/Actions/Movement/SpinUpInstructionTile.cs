using Mona.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinUpInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Up";
        public const string NAME = "Spin Up";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(SpinUpInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinUp;
    }
}