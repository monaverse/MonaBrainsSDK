using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceUpInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceUp";
        public const string NAME = "Apply Force Up";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceUpInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Up;
    }
}