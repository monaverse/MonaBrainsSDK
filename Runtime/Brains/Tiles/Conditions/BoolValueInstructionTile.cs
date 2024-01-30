using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class BoolValueInstructionTile : InstructionTile, IInstructionTileWithPreload, IBoolValueInstructionTile, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "BoolValue";
        public const string NAME = "Bool Value Is";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(BoolValueInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaStateBoolValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] private BoolValueOperatorType _operator = BoolValueOperatorType.Equal;
        [BrainPropertyEnum(false)] public BoolValueOperatorType Operator { get => _operator; set => _operator = value; }

        [SerializeField] private bool _value;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public bool Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value")] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;

        public BoolValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.State.GetBool(_valueValueName);

            if (_brain != null && Evaluate(_brain.State))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainState state)
        {
            switch(_operator)
            {
                case BoolValueOperatorType.NotEqual: return state.GetBool(_valueName) != _value;
                default: return state.GetBool(_valueName) == _value;
            }
        }

    }
}