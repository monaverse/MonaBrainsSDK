using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongZNegativeInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along Z Negative";
        public const string NAME = "Move Along Z Negative";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongZNegativeInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.ZNegative;
    }
}