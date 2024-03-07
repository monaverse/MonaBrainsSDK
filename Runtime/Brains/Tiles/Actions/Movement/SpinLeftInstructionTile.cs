using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SpinLeftInstructionTile : RotateLocalInstructionTile, IRotateLocalInstructionTile
    {
        public const string ID = "Spin Left";
        public const string NAME = "Spin Left";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(SpinLeftInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.SpinLeft;

        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Angle")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Angle")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Instant, "Angle")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.PerSecondMovement, "Angles/Sec")]
        [BrainProperty(true)] public float Angle { get => _angle; set => _angle = value; }
        [BrainPropertyValueName("Angle", typeof(IMonaVariablesFloatValue))] public string AngleValueName { get => _angleValueName; set => _angleValueName = value; }

    }
}