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
    public class SetAsTriggerInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetAsTrigger";
        public const string NAME = "Is Trigger Volume";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetAsTriggerInstructionTile);

        [SerializeField] private bool _value;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public bool Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesBoolValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;
        private Collider _collider;

        public SetAsTriggerInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            if (_collider != null)
                return;

            if(!_brain.Body.HasCollider())
                _brain.Body.AddCollider();
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetBool(_valueValueName);

            _brain.Body.SetTriggerVolumeState(_value);
            return Complete(InstructionTileResult.Success);
        }
    }
}