using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RollRightInstructionTile : RotateLocalInstructionTile, IRotateLocalInstructionTile
    {
        public const string ID = "Roll Right";
        public const string NAME = "Roll Right";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(RollRightInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.RollRight;
    }
}