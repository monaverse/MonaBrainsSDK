using Mona.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveUpInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Up";
        public const string NAME = "Move Up";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveUpInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Up;
    }
}