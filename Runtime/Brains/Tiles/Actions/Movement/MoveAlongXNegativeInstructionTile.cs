using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongXNegativeInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along X Negative";
        public const string NAME = "Move Along X Negative";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongXNegativeInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.XNegative;
    }
}