using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RollLeftInstructionTile : RotateLocalInstructionTile, IRotateLocalInstructionTile
    {
        public const string ID = "Roll Left";
        public const string NAME = "Roll Left";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(RollLeftInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.RollLeft;
    }
}