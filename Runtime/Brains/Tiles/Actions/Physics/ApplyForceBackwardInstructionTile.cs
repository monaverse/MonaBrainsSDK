using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceBackwardInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceBackward";
        public const string NAME = "Apply Force Backward";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceBackwardInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Backward;
    }
}