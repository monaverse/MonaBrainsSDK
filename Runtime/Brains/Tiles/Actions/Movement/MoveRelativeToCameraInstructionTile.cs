using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveRelativeToCameraInstructionTile : MoveLocalWithDistanceInstructionTile
    {
        public const string ID = "Move Relative To Camera";
        public const string NAME = "Move Relative To Camera";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveRelativeToCameraInstructionTile);

        public override MoveDirectionType DirectionType => (MoveDirectionType)(20 + (int)_cameraDirection);

        [SerializeField] public MoveCameraDirectionType _cameraDirection = MoveCameraDirectionType.UseInput;
        [BrainPropertyEnum(false)] public MoveCameraDirectionType CameraDirection { get => _cameraDirection; set => _cameraDirection = value; }
    }
}