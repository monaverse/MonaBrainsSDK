using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceAlignToDirectionInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceAlignToDirection";
        public const string NAME = "Force Maintain Distance";
        public const string CATEGORY = "Forces";
        public override Type TileType => typeof(ApplyForceAlignToDirectionInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.PositionalAlignment;
        public override PositionalAlignmentMode AlignmentMode => PositionalAlignmentMode.Direction;
    }
}