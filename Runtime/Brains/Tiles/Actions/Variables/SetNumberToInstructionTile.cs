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
    public class SetNumberToInstructionTile : InstructionTile, ISetNumberToInstructionTile, IActionInstructionTile,
        IStoreVariableInstructionTile

    {
        public const string ID = "SetNumberTo";
        public const string NAME = "SetNumberTo";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetNumberToInstructionTile);

        [SerializeField] private VariableTargetToStoreResult _setResultTo = VariableTargetToStoreResult.SameVariable;
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Add)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Subtract)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Multiply)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Divide)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Exponent)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SquareRoot)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Modulo)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.RoundUp)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.RoundDown)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.RoundClosest)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SetPositive)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SetNegative)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SetToMax)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SetToMin)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.SetToDefault)]
        [BrainPropertyEnum(false)] public VariableTargetToStoreResult SetResultTo
        {
            get => GetOperator() == ValueChangeType.Set ? VariableTargetToStoreResult.SameVariable : _setResultTo;
            set => _setResultTo = value;
        }

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private float _amount = 1;
        [SerializeField] private string _amountValueName;
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Set)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Add)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Subtract)]
        [BrainProperty(true)] public float Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount", typeof(IMonaVariablesFloatValue))] public string AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        [SerializeField] private float _by = 2;
        [SerializeField] private string _byValueName;
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Multiply)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Divide)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Exponent)]
        [BrainPropertyShow(nameof(OperatorToUse), (int)ValueChangeType.Modulo)]
        [BrainProperty(true)] public float By { get => _by; set => _by = value; }
        [BrainPropertyValueName("By", typeof(IMonaVariablesFloatValue))] public string ByValueName { get => _byValueName; set => _byValueName = value; }

        [SerializeField] private string _storeResultOn;
        [BrainPropertyShow(nameof(SetResultTo), (int)VariableTargetToStoreResult.OtherVariable)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string StoreResultOn { get => _storeResultOn; set => _storeResultOn = value; }

        private IMonaBrain _brain;

        public ValueChangeType OperatorToUse => GetOperator();

        private float AmountToUse
        {
            get
            {
                switch (GetOperator())
                {
                    case ValueChangeType.Multiply:
                        return _by;
                    case ValueChangeType.Divide:
                        return _by;
                    case ValueChangeType.Exponent:
                        return _by;
                    case ValueChangeType.Modulo:
                        return _by;
                }

                return _amount;
            }
        }

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

            if (!string.IsNullOrEmpty(_byValueName))
                _by = _brain.Variables.GetFloat(_byValueName);

            if (_brain == null || string.IsNullOrEmpty(_numberName) || (SetResultTo == VariableTargetToStoreResult.OtherVariable && string.IsNullOrEmpty(_storeResultOn)))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (Evaluate(_brain.Variables))
            {
                //if(_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(SetNumberToInstructionTile)} {_operator} {_amount} = {_brain.Variables.GetFloat(_valueName)}");
                return Complete(InstructionTileResult.Success);
            }

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_numberName);
            if (variable == null)
            {
                state.Set(_numberName, AmountToUse);
                return true;
            }

            string nameOfVariableToSet = SetResultTo == VariableTargetToStoreResult.SameVariable ?
                _numberName : _storeResultOn;

            if (variable is IMonaVariablesFloatValue)
                ChangeFloatValue(state, nameOfVariableToSet, ((IMonaVariablesFloatValue)variable).Value, GetOperator(), AmountToUse);
            else if(variable is IMonaVariablesVector2Value)
                ChangeVector2Value(state, nameOfVariableToSet, ((IMonaVariablesVector2Value)variable).Value, GetOperator(), AmountToUse);
            else if (variable is IMonaVariablesVector3Value)
                ChangeVector3Value(state, nameOfVariableToSet, ((IMonaVariablesVector3Value)variable).Value, GetOperator(), AmountToUse);
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
                case ValueChangeType.Exponent:
                    state.Set(name, Mathf.Pow(value, amount));
                    break;
                case ValueChangeType.SquareRoot:
                    if (value >= 0)
                        state.Set(name, Mathf.Sqrt(value));
                    break;
                case ValueChangeType.Modulo:
                    state.Set(name, value % amount);
                    break;
                case ValueChangeType.RoundUp:
                    state.Set(name, Mathf.Ceil(value));
                    break;
                case ValueChangeType.RoundDown:
                    state.Set(name, Mathf.Floor(value));
                    break;
                case ValueChangeType.RoundClosest:
                    state.Set(name, Mathf.Round(value));
                    break;
                case ValueChangeType.SetPositive:
                    state.Set(name, value < 0 ? value * -1f : value);
                    break;
                case ValueChangeType.SetNegative:
                    state.Set(name, value > 0 ? value * -1f : value);
                    break;
                case ValueChangeType.SetToMax:
                    float max = ((IMonaVariablesFloatValue)state.GetVariable(_numberName)).Max;
                    state.Set(name, max);
                    break;
                case ValueChangeType.SetToMin:
                    float min = ((IMonaVariablesFloatValue)state.GetVariable(_numberName)).Min;
                    state.Set(name, min);
                    break;
                case ValueChangeType.SetToDefault:
                    float defaultValue = ((IMonaVariablesFloatValue)state.GetVariable(_numberName)).DefaultValue;
                    state.Set(name, defaultValue);
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