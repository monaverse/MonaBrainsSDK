using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueRollLeftInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueRollLeft";
        public const string NAME = "Torque Roll Left";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueRollLeftInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.RollLeft;
    }
}