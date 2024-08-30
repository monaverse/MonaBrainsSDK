using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceNadirInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceNadir";
        public const string NAME = "Apply Force Nadir";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceNadirInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Up;
        public override SpaceType Space => SpaceType.World;
    }
}