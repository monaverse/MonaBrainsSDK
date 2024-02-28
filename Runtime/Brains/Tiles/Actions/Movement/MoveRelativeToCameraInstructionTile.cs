using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveRelativeToCameraInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move Relative To Camera";
        public const string NAME = "Move Relative To Camera";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveRelativeToCameraInstructionTile);

        public override MoveDirectionType DirectionType => (MoveDirectionType)(int)_cameraDirection;

        [SerializeField] public MoveCameraDirectionType _cameraDirection = MoveCameraDirectionType.CameraForward;
        [BrainPropertyEnum(true)] public MoveCameraDirectionType CameraDirection { get => _cameraDirection; set => _cameraDirection = value; }

    }
}