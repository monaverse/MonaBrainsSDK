using Mona.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveDownInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Down";
        public const string NAME = "Move Down";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveDownInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Down;
    }
}