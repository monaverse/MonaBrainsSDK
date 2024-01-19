using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveBackwardInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Backward";
        public const string NAME = "Move Backward";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveBackwardInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Backward;
    }
}