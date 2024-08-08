using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetAsTriggerInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
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
        private List<Collider> _colliders;

        public SetAsTriggerInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            if (!_brain.Body.HasCollider())
                _colliders = _brain.Body.AddCollider();
            else
                _colliders = _brain.Body.Colliders;
        }

        public override void Unload(bool destroy = false)
        {
            if (destroy)
            {
                if (_colliders != null)
                {
                    for (var i = 0; i < _colliders.Count; i++)
                        GameObject.Destroy(_colliders[i]);
                }
                _colliders = null;
            }
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