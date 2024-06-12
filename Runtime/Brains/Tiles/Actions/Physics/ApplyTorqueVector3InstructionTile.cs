using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyTorqueVector3InstructionTile : ApplyTorqueLocalInstructionTile
    {
        public const string ID = "ApplyTorqueVector3";
        public const string NAME = "Apply Torque Vector3";
        public const string CATEGORY = "Torque";
        public override Type TileType => typeof(ApplyTorqueVector3InstructionTile);

        [SerializeField] private Vector3 _directionValue;
        [SerializeField] private string[] _directionName;

        [BrainProperty(true)] public Vector3 Direction { get => _directionValue; set => _directionValue = value; }
        [BrainPropertyValueName("Direction", typeof(IMonaVariablesVector3Value))] public string[] DirectionName { get => _directionName; set => _directionName = value; }

        public override PushDirectionType DirectionType => PushDirectionType.Custom;

        protected override Vector3 GetDirectionVector(PushDirectionType moveType, IMonaBody body)
        {
            if (HasVector3Values(_directionName))
                _directionValue = GetVector3Value(_brain, _directionName);
            return _directionValue;
        }
    }
}