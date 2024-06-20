using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceAlignHoverInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceAlignHover";
        public const string NAME = "Apply Hover Force";
        public const string CATEGORY = "Forces";
        public override Type TileType => typeof(ApplyForceAlignHoverInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.PositionalAlignment;
        public override PositionalAlignmentMode AlignmentMode => PositionalAlignmentMode.Hover;
    }
}