using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Tiles.Actions.General.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeValueInstructionTile : InstructionTile, IChangeValueInstructionTile, IActionInstructionTile
    {
        public const string ID = "ChangeValue";
        public const string NAME = "ChangeValue";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeValueInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] private ValueChangeType _operator = ValueChangeType.Set;
        [BrainPropertyEnum(false)] public ValueChangeType Operator { get => _operator; set => _operator = value; }

        [SerializeField] private float _amount;
        [SerializeField] private string _amountValueName;
        [BrainProperty(true)] public float Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount")] public string AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        private IMonaBrain _brain;

        public ChangeValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_amountValueName))
                _amount = _brain.Variables.GetFloat(_amountValueName);

            if (_brain != null)
            {
                if(Evaluate(_brain.Variables))
                    return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_valueName);
            if (variable == null)
            {
                state.Set(_valueName, _amount);
                return true;
            }

            if (variable is IMonaVariablesFloatValue)
                ChangeFloatValue(state, _valueName, ((IMonaVariablesFloatValue)variable).Value, _operator, _amount);
            else if(variable is IMonaVariablesVector2Value)
                ChangeVector2Value(state, _valueName, ((IMonaVariablesVector2Value)variable).Value, _operator, _amount);
            else if (variable is IMonaVariablesVector3Value)
                ChangeVector3Value(state, _valueName, ((IMonaVariablesVector3Value)variable).Value, _operator, _amount);
            return true;
        }

        private void ChangeFloatValue(IMonaBrainVariables state, string name, float value, ValueChangeType op, float amount)
        {
            switch (op)
            {
                case ValueChangeType.Add: 
                    state.Set(name, value + amount); 
                    break;
                case ValueChangeType.Subtract: 
                    state.Set(name, value - amount); 
                    break;
                case ValueChangeType.Divide: 
                    if (amount != 0) 
                        state.Set(name, value / amount); 
                    break;
                case ValueChangeType.Multiply: 
                    state.Set(name, value * amount); 
                    break;
                default: 
                    state.Set(name, amount); 
                    break;
            }
        }

        private void ChangeVector2Value(IMonaBrainVariables state, string name, Vector2 value, ValueChangeType op, float amount)
        {
            var vectorAmount = new Vector2(amount, amount);
            switch (op)
            {
                case ValueChangeType.Add:
                    state.Set(name, value + vectorAmount); 
                    break;
                case ValueChangeType.Subtract: 
                    state.Set(name, value - vectorAmount); 
                    break;
                case ValueChangeType.Divide: 
                    if (vectorAmount.x != 0f && vectorAmount.y != 0f) 
                        state.Set(name, value / vectorAmount); 
                    break;
                case ValueChangeType.Multiply: 
                    state.Set(name, value * vectorAmount); 
                    break;
                default: 
                    state.Set(name, vectorAmount); 
                    break;
            }
        }

        private void ChangeVector3Value(IMonaBrainVariables state, string name, Vector3 value, ValueChangeType op, float amount)
        {
            var vectorAmount = new Vector3(amount, amount, amount);
            switch (op)
            {
                case ValueChangeType.Add:
                    state.Set(name, value + vectorAmount); 
                    break;
                case ValueChangeType.Subtract: 
                    state.Set(name, value - vectorAmount); 
                    break;
                case ValueChangeType.Divide: 
                    if (amount != 0f) 
                        state.Set(name, value / amount); 
                    break;
                case ValueChangeType.Multiply:
                    state.Set(name, value * amount); break;
                    break;
                default: 
                    state.Set(name, vectorAmount); 
                    break;
            }
        }
    }
}