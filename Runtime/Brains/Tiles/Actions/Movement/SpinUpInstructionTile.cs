using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinUpInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Up";
        public const string NAME = "Spin Up";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(SpinUpInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinUp;
    }
}