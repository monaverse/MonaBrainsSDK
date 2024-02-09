using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateAroundInputInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "RotateAroundInput";
        public const string NAME = "Rotate Around Input";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(RotateAroundInputInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.InputLeftRight;

        [BrainProperty(false)] public bool MovingOnlyTurn { get => _onlyTurnWhenMoving; set => _onlyTurnWhenMoving = value; }

    }
}