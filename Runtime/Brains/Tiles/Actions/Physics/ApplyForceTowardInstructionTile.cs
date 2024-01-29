using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceTowardInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceToward";
        public const string NAME = "Apply Force Toward";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceTowardInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Toward;
    }
}