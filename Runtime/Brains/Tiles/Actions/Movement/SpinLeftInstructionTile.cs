using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinLeftInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Left";
        public const string NAME = "Spin Left";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(SpinLeftInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinLeft;
    }
}