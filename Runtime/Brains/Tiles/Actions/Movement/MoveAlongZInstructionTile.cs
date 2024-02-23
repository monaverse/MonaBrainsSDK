using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongZInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Along Z";
        public const string NAME = "Move Along Z";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongZInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Z;
    }
}