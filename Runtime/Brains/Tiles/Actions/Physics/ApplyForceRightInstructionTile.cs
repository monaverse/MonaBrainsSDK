using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceRightInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceRight";
        public const string NAME = "Apply Force Right";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceRightInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Right;
    }
}