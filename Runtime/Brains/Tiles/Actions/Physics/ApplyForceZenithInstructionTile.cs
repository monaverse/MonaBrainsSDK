using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceZenithInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceZenith";
        public const string NAME = "Apply Force Zenith";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceZenithInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Up;
        public override SpaceType Space => SpaceType.World;
    }
}