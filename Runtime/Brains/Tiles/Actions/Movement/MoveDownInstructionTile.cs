using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveDownInstructionTile : MoveLocalWithDistanceInstructionTile
    {
        public const string ID = "Move Down";
        public const string NAME = "Move Down";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveDownInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Down;
    }
}