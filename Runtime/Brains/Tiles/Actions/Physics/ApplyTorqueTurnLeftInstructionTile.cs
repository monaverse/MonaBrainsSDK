using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueTurnLeftInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueTurnLeft";
        public const string NAME = "Torque Turn Left";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueTurnLeftInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.TurnLeft;
    }
}