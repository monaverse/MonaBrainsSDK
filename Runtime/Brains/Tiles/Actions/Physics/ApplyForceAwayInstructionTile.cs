using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceAwayInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceAway";
        public const string NAME = "Apply Force Away";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceAwayInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Away;
    }
}