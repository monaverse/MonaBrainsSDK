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
    public class SetNumberToInstructionTile : InstructionTile, ISetNumberToInstructionTile, IActionInstructionTile
    {
        public const string ID = "SetNumberTo";
        public const string NAME = "SetNumberTo";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetNumberToInstructionTile);

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private float _amount;
        [SerializeField] private string _amountValueName;
        [BrainProperty(true)] public float Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount", typeof(IMonaVariablesFloatValue))] public string AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        private IMonaBrain _brain;

        public SetNumberToInstructionTile() { }

        protected virtual ValueChangeType GetOperator()
        {
            return ValueChangeType.Set;
        }

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
                //Debug.Log($"{nameof(SetNumberToInstructionTile)} {_amount} = {_brain.Variables.GetFloat(_numberName)}", _brain.Body.Transform.gameObject);
                if (Evaluate(_brain.Variables))
                {
                    //if(_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(SetNumberToInstructionTile)} {_operator} {_amount} = {_brain.Variables.GetFloat(_valueName)}");
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_numberName);
            if (variable == null)
            {
                state.Set(_numberName, _amount);
                return true;
            }

            if (variable is IMonaVariablesFloatValue)
                ChangeFloatValue(state, _numberName, ((IMonaVariablesFloatValue)variable).Value, GetOperator(), _amount);
            else if(variable is IMonaVariablesVector2Value)
                ChangeVector2Value(state, _numberName, ((IMonaVariablesVector2Value)variable).Value, GetOperator(), _amount);
            else if (variable is IMonaVariablesVector3Value)
                ChangeVector3Value(state, _numberName, ((IMonaVariablesVector3Value)variable).Value, GetOperator(), _amount);
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