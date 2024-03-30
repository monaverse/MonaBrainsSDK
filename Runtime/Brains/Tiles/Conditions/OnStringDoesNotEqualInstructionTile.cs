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
    public class OnStringDoesNotEqualInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnStringDoesNotEqual";
        public const string NAME = "String != Equal";
        public const string CATEGORY = "Strings";
        public override Type TileType => typeof(OnStringDoesNotEqualInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] protected string _toCheck;
        [SerializeField] protected string _toCheckName;
        [BrainProperty(true)] public string ToCheck { get => _toCheck; set => _toCheck = value; }
        [BrainPropertyValueName("ToCheck", typeof(IMonaVariablesValue))] public string ToCheckName { get => _toCheckName; set => _toCheckName = value; }

        private IMonaBrain _brain;

        protected virtual ValueOperatorType GetOperator()
        {
            return ValueOperatorType.Equal;
        }

        public OnStringDoesNotEqualInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_toCheckName))
                _toCheck = _brain.Variables.GetValueAsString(_toCheckName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            string stringValue = _brain.Variables.GetString(_valueName);

            return stringValue != _toCheck ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}