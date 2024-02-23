using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongYInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along Y";
        public const string NAME = "Move Along Y";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongYInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Y;
    }
}