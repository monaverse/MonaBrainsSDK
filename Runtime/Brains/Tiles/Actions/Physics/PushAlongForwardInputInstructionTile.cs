using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class PushAlongForwardInputInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "PushAlongForwardInput";
        public const string NAME = "Push Along Forward Input";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(PushAlongForwardInputInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.InputForwardBack;
    }
}