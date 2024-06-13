using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorquePitchUpInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorquePitchUp";
        public const string NAME = "Torque Pitch Up";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorquePitchUpInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.PitchUp;
    }
}