using UnityEngine;
using System;
using System.Collections;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetWorldGravityInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "SetGravity";
        public const string NAME = "World Gravity";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetWorldGravityInstructionTile);

        [SerializeField] private GravityValueType _mode = GravityValueType.Strength;
        [BrainPropertyEnum(false)] public GravityValueType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _strength = 1f;
        [SerializeField] private string _strengthName;
        [BrainPropertyShow(nameof(Mode), (int)GravityValueType.Strength)]
        [BrainProperty(true)] public float Strength { get => _strength; set => _strength = value; }
        [BrainPropertyValueName("Strength", typeof(IMonaVariablesFloatValue))] public string StrengthName { get => _strengthName; set => _strengthName = value; }

        [SerializeField] private Vector3 _direction = new Vector3 (0, -1, 0);
        [SerializeField] private string[] _directionName;
        [BrainPropertyShow(nameof(Mode), (int)GravityValueType.DirectionVector)]
        [BrainProperty(true)] public Vector3 Direction { get => _direction; set => _direction = value; }
        [BrainPropertyValueName("Direction", typeof(IMonaVariablesVector3Value))] public string[] DirectionName { get => _directionName; set => _directionName = value; }

        private IMonaBrain _brain;

        [Serializable]
        public enum GravityValueType
        {
            Strength = 0,
            DirectionVector = 10
        }

        public SetWorldGravityInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE); ;

            if (!string.IsNullOrEmpty(_strengthName))
                _strength = _brain.Variables.GetFloat(_strengthName);

            if (HasVector3Values(_directionName))
                _direction = GetVector3Value(_brain, _directionName);

            Vector3 gravityDirection = _mode == GravityValueType.Strength ?
                new Vector3(0, _strength * -1f, 0) :
                _direction;

            UnityEngine.Physics.gravity = gravityDirection;

            return Complete(InstructionTileResult.Success);
        }
    }
}