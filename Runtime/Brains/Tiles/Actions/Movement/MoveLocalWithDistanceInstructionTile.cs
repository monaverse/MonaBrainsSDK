using UnityEngine;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Core;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveLocalWithDistanceInstructionTile : MoveLocalInstructionTile
    {
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Meters")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Meters")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Instant, "Meters")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.SpeedOnly, "Meters/Sec")]
        [BrainProperty(true)]
        public float Distance { get => _distance; set => _distance = value; }

        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))]
        public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }
    }
}


