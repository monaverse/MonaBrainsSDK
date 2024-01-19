using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinDownInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "Spin Down";
        public const string NAME = "Spin Down";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(SpinDownInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinDown;
    }
}