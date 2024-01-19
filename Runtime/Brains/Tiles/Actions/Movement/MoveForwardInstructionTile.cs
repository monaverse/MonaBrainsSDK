using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveForwardInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Forward";
        public const string NAME = "Move Forward";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveForwardInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Forward;
    }
}