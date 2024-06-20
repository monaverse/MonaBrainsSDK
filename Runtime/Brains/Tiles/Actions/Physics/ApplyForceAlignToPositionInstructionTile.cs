using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceAlignToPositionInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceAlignToPosition";
        public const string NAME = "Stabilize Position";
        public const string CATEGORY = "Forces";
        public override Type TileType => typeof(ApplyForceAlignToPositionInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.PositionalAlignment;
        public override PositionalAlignmentMode AlignmentMode => PositionalAlignmentMode.TargetPosition;
    }
}