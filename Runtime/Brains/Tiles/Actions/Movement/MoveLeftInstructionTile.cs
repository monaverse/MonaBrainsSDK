using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveLeftInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Left";
        public const string NAME = "Move Left";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveLeftInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Left;
    }
}