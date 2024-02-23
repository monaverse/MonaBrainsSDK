using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongXInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along X";
        public const string NAME = "Move Along X";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongXInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.X;
    }
}