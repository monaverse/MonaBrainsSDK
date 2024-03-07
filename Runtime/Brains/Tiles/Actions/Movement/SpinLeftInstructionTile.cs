using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinLeftInstructionTile : RotateLocalInstructionTile, IRotateLocalInstructionTile
    {
        public const string ID = "Spin Left";
        public const string NAME = "Spin Left";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(SpinLeftInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinLeft;
    }
}