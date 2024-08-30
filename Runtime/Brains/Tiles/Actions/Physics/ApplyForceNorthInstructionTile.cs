using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceNorthInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceNorth";
        public const string NAME = "Apply Force North";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceNorthInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Forward;
        public override SpaceType Space => SpaceType.World;
    }
}