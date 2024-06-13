using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueTurnRightInstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueTurnRight";
        public const string NAME = "Torque Turn Right";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueTurnRightInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.TurnRight;
    }
}