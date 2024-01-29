using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceLeftInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceLeft";
        public const string NAME = "Apply Force Left";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceLeftInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Left;
    }
}