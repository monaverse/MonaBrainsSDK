using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongForwardInputInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "MoveAlongForward";
        public const string NAME = "Move Along Forward Input";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveAlongForwardInputInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.InputForwardBack;
    }
}