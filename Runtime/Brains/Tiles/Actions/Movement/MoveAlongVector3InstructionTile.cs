using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveAlongVector3InstructionTile : MoveLocalWithDistanceInstructionTile
    {
        public const string ID = "Move Along Vector3";
        public const string NAME = "Move Along Vector3";
        public const string CATEGORY = "Global Movement";
        public override Type TileType => typeof(MoveAlongVector3InstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.Custom;

        [SerializeField] private Vector3 _directionValue;
        [SerializeField] private string[] _directionName;

        [BrainProperty(true)] public Vector3 Direction { get => _directionValue; set => _directionValue = value; }
        [BrainPropertyValueName("Direction", typeof(IMonaVariablesVector3Value))] public string[] DirectionName { get => _directionName; set => _directionName = value; }

        protected override Vector3 GetDirectionVector(MoveDirectionType moveType)
        {
            if (HasVector3Values(_directionName))
                _directionValue = GetVector3Value(_brain, _directionName);
            return _directionValue.normalized;
        }
    }
}