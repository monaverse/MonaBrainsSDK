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
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberNegativeInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnNumberNegative";
        public const string NAME = "NumberIsNegative";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(OnNumberNegativeInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        private IMonaBrain _brain;

        public OnNumberNegativeInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && Evaluate(_brain.Variables))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_valueName);
            if(variable is IMonaVariablesFloatValue)
                return EvaluateValue((IMonaVariablesFloatValue)variable);
            return false;
        }

        private bool EvaluateValue(IMonaVariablesFloatValue value)
        {
            return value.Value < 0;
        }
    }
}