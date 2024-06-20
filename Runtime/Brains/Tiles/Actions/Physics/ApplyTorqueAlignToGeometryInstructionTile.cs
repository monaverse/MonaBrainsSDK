using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueAlignToGeometryInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueGeometryAlign";
        public const string NAME = "Torque Geometry Align";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueAlignToGeometryInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.TorqueAlignment;
        public override TorqueAlignmentMode AlignmentMode => TorqueAlignmentMode.GeometryInDirection;
    }
}