using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueAlignGroundInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueAlignGround";
        public const string NAME = "Torque Ground Align";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueAlignGroundInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.TorqueAlignment;
        public override TorqueAlignmentMode AlignmentMode => TorqueAlignmentMode.GroundAngle;
    }
}