using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class PushInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "Push";
        public const string NAME = "Push";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(PushInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Push;
    }
}