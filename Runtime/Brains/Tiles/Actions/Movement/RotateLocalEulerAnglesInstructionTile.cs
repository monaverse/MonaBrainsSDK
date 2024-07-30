using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalEulerAnglesInstructionTile : RotateLocalInstructionTile, IRotateLocalInstructionTile
    {
        public const string ID = "RotateToAngles";
        public const string NAME = "Rotate To Angles";
        public const string CATEGORY = "Adv Rotation";
        public override Type TileType => typeof(RotateLocalEulerAnglesInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.EulerAngles;

        //[BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Angle")]
        //[BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Angle")]
        //[BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Instant, "Angle")]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.SpeedOnly)]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.SpeedOnly, "Angles/Sec")]
        [BrainProperty(true)] public float Angle { get => _angle; set => _angle = value; }
        [BrainPropertyValueName("Angle", typeof(IMonaVariablesFloatValue))] public string AngleValueName { get => _angleValueName; set => _angleValueName = value; }

    }
}