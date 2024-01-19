using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveRightInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Right";
        public const string NAME = "Move Right";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveRightInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Right;
    }
}