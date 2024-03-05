using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongYNegativeInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along Y Negative";
        public const string NAME = "Move Along Y Negative";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongYNegativeInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.YNegative;
    }
}