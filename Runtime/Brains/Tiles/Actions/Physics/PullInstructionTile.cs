using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class PullInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "Pull";
        public const string NAME = "Pull";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(PullInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Pull;
    }
}