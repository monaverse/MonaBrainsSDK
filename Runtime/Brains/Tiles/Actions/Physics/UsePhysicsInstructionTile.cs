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
    public class UsePhysicsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UsePhysicsInstructionTile";
        public const string NAME = "Use Physics";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(UsePhysicsInstructionTile);

        [SerializeField] private bool _value;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public bool Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesBoolValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;

        public UsePhysicsInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetBool(_valueValueName);

            if (_brain != null)
            {
                _brain.Body.SetKinematic(!_value);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}