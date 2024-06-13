using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueRollRightInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueRollRight";
        public const string NAME = "Torque Roll Right";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueRollRightInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.RollRight;
    }
}