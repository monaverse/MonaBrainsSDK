using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceWestInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceWest";
        public const string NAME = "Apply Force West";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceWestInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Left;
        public override SpaceType Space => SpaceType.World;
    }
}