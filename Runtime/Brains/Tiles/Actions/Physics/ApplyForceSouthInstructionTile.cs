using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceSouthInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceSouth";
        public const string NAME = "Apply Force South";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceSouthInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Backward;
        public override SpaceType Space => SpaceType.World;
    }
}