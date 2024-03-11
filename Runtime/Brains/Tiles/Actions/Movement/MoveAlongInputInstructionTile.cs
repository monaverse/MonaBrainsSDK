using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongInputInstructionTile : MoveLocalWithDistanceInstructionTile
    {
        public const string ID = "MoveAlongInput";
        public const string NAME = "Move Along Input";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongInputInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.UseInput;
    }
}