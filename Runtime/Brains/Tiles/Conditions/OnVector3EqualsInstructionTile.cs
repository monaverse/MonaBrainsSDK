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
    public class OnVector3EqualsInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnVector3Equals";
        public const string NAME = "Vector3 = Equals";
        public const string CATEGORY = "Vectors";
        public override Type TileType => typeof(OnVector3EqualsInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] private Vector3 _amount;
        [SerializeField] private string[] _amountValueName;
        [BrainProperty(true)] public Vector3 Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount", typeof(IMonaVariablesVector3Value))] public string[] AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        private IMonaBrain _brain;

        protected virtual ValueOperatorType GetOperator()
        {
            return ValueOperatorType.Equal;
        }

        public OnVector3EqualsInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_valueName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (HasVector3Values(_amountValueName))
                _amount = GetVector3Value(_brain, _amountValueName);

            Vector3 value = _brain.Variables.GetVector3(_valueName);

            if (value == _amount)
                return Complete(InstructionTileResult.Success);
            else
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}