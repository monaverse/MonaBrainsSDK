using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceForwardInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceForward";
        public const string NAME = "Apply Force Forward";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceForwardInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Forward;
    }
}