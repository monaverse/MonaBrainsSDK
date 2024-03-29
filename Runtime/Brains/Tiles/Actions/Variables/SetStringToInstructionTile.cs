using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SetStringToInstructionTile : InstructionTile, ISetStringToInstructionTile, IActionInstructionTile
    {
        public const string ID = "SetEquals";
        public const string NAME = "Set Equals = ";
        public const string CATEGORY = "Strings";

        public override Type TileType => typeof(SetStringToInstructionTile);

        public virtual ValueChangeType Operator => ValueChangeType.Set;

        [SerializeField] protected string _stringName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string StringName { get => _stringName; set => _stringName = value; }

        [SerializeField] protected string _value;
        [SerializeField] protected string _valueName;
        [BrainProperty(true)] public string Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesValue))] public string ValueName { get => _valueName; set => _valueName = value; }

        protected IMonaBrain _brain;

        public SetStringToInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueName))
                _value = _brain.Variables.GetValueAsString(_valueName);

            if (_brain != null)
            {
                if (Evaluate(_brain.Variables))
                    return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        protected virtual bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_stringName);

            if (variable == null)
            {
                state.Set(_stringName, _value);
                return true;
            }

            if (!(variable is IMonaVariablesStringValue))
                return false;
            
            string variableValue = ((IMonaVariablesStringValue)variable).Value;

            state.Set(_stringName, _value);

            switch (Operator)
            {
                case ValueChangeType.Add:
                    state.Set(_stringName, variableValue + _value);
                    break;
                case ValueChangeType.Subtract:
                    string subtractedValue = variableValue.Replace(_value, "");
                    state.Set(_stringName, subtractedValue);
                    break;
                default:
                    state.Set(_stringName, _value);
                    break;
            }

            return true;
        }
    }
}