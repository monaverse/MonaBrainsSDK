using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorquePitchDownInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorquePitchDown";
        public const string NAME = "Torque Pitch Down";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorquePitchDownInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.PitchDown;
    }
}