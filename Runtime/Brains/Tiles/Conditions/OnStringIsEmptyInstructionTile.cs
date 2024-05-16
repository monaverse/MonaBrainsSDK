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
    public class OnStringIsEmptyInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnStringIsEmpty";
        public const string NAME = "String Is Empty";
        public const string CATEGORY = "Strings";
        public override Type TileType => typeof(OnStringIsEmptyInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        private IMonaBrain _brain;

        protected virtual ValueOperatorType GetOperator()
        {
            return ValueOperatorType.Equal;
        }

        public OnStringIsEmptyInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            string stringValue = _brain.Variables.GetValueAsString(_valueName);

            return string.IsNullOrEmpty(stringValue) ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}