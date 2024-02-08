﻿using UnityEngine;
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
    public class OnValueInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueInstructionTile, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnValue";
        public const string NAME = "Value Is";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(OnValueInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] private ValueOperatorType _operator = ValueOperatorType.Equal;
        [BrainPropertyEnum(false)] public ValueOperatorType Operator { get => _operator; set => _operator = value; }

        [SerializeField] private float _amount;
        [SerializeField] private string _amountValueName;
        [BrainProperty(true)] public float Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount", typeof(IMonaVariablesFloatValue))] public string AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        private IMonaBrain _brain;

        public OnValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_amountValueName))
                _amount = _brain.Variables.GetFloat(_amountValueName);

            if (_brain != null && Evaluate(_brain.Variables))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            switch(_operator)
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