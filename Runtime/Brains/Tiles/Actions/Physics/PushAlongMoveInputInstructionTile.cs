using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class PushAlongMoveInputInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "PushAlongMoveInput";
        public const string NAME = "Push Along Move Input";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(PushAlongMoveInputInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.UseInput;
    }
}