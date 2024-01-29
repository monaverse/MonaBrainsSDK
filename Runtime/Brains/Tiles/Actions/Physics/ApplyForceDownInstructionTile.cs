using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceDownInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceDown";
        public const string NAME = "Apply Force Down";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceDownInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Down;
    }
}