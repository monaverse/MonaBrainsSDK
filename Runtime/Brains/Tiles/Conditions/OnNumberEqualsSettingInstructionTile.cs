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
    public class OnNumberEqualsSettingInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnNumberEqualsSetting";
        public const string NAME = "Number Equals Setting";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(OnNumberEqualsSettingInstructionTile);

        [SerializeField] MonaBrainFloatSettingValueType _floatSetting = MonaBrainFloatSettingValueType.Default;

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        private float _amount;

        private IMonaBrain _brain;

        protected virtual ValueOperatorType GetOperator()
        {
            return ValueOperatorType.Equal;
        }

        public OnNumberEqualsSettingInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (string.IsNullOrEmpty(_valueName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var variable = _brain.Variables.GetVariable(_valueName);

            if (variable == null || !((IMonaVariablesFloatValue)variable).UseMinMax)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            switch (_floatSetting)
            {
                case MonaBrainFloatSettingValueType.Min:
                    _amount = ((IMonaVariablesFloatValue)variable).Min;
                    break;
                case MonaBrainFloatSettingValueType.Max:
                    _amount = ((IMonaVariablesFloatValue)variable).Max;
                    break;
                default:
                    _amount = ((IMonaVariablesFloatValue)variable).DefaultValue;
                    break;
            }

            if (_brain != null && Evaluate(_brain.Variables))
                return Complete(InstructionTileResult.Success);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            switch(GetOperator())
            {
                case ValueOperatorType.GreaterThanEqual: return state.GetFloat(_valueName) >= _amount;
                case ValueOperatorType.GreaterThan: return state.GetFloat(_valueName) > _amount;
                case ValueOperatorType.LessThanEqual: return state.GetFloat(_valueName) <= _amount;
                case ValueOperatorType.LessThan: return state.GetFloat(_valueName) < _amount;
                case ValueOperatorType.NotEqual: return state.GetFloat(_valueName) != _amount;
                default: return state.GetFloat(_valueName) == _amount;
            }
        }

    }
}