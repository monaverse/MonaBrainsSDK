using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetMassInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetMass";
        public const string NAME = "Set Mass";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetMassInstructionTile);

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public float Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;

        public SetMassInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            _brain.Body.AddRigidbody();
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _brain.Body.ActiveRigidbody == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE); ;

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            _brain.Body.ActiveRigidbody.mass = _value;

            return Complete(InstructionTileResult.Success);
        }
    }
}