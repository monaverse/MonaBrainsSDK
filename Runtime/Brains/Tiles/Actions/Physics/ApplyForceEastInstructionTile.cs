using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceEastInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceEast";
        public const string NAME = "Apply Force East";
        public const string CATEGORY = "Global Forces";
        public override Type TileType => typeof(ApplyForceEastInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Right;
        public override SpaceType Space => SpaceType.World;
    }
}