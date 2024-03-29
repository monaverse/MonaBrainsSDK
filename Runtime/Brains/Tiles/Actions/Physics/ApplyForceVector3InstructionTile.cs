using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceVector3InstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceVector3";
        public const string NAME = "Apply Force Vector3";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceVector3InstructionTile);

        [SerializeField] private Vector3 _direction;
        [SerializeField] private string[] _directionName;

        [BrainProperty(true)] public Vector3 Direction { get => _direction; set => _direction = value; }
        [BrainPropertyValueName("Direction", typeof(IMonaVariablesVector3Value))] public string[] DirectionName { get => _directionName; set => _directionName = value; }

        public override PushDirectionType DirectionType => PushDirectionType.Custom;

        protected override Vector3 GetDirectionVector(PushDirectionType moveType, IMonaBody body)
        {
            if (HasVector3Values(_directionName))
                _direction = GetVector3Value(_brain, _directionName);
            return _direction;
        }
    }
}