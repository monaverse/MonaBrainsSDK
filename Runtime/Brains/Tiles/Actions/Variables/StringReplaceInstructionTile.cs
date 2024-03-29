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
    public class StringReplaceInstructionTile : InstructionTile, IStringReplaceInstructionTile, IActionInstructionTile
    {
        public const string ID = "StringReplace";
        public const string NAME = "Find and Replace";
        public const string CATEGORY = "Strings";
        public override Type TileType => typeof(StringReplaceInstructionTile);

        [SerializeField] private string _stringName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string StringName { get => _stringName; set => _stringName = value; }

        [SerializeField] private string _replaceThis;
        [SerializeField] private string _replaceThisName;
        [BrainProperty(true)] public string ReplaceThis { get => _replaceThis; set => _replaceThis = value; }
        [BrainPropertyValueName("ReplaceThis", typeof(IMonaVariablesValue))] public string ReplaceThisName { get => _replaceThisName; set => _replaceThisName = value; }

        [SerializeField] private string _withThis;
        [SerializeField] private string _withThisName;
        [BrainProperty(true)] public string WithThis { get => _withThis; set => _withThis = value; }
        [BrainPropertyValueName("WithThis", typeof(IMonaVariablesValue))] public string WithThisName { get => _withThisName; set => _withThisName = value; }

        private IMonaBrain _brain;

        public StringReplaceInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_replaceThisName))
                _replaceThis = _brain.Variables.GetValueAsString(_replaceThisName);

            if (!string.IsNullOrEmpty(_withThisName))
                _withThis = _brain.Variables.GetValueAsString(_withThisName);

            if (_brain != null)
            {
                var variable = _brain.Variables.GetVariable(_stringName);

                if (variable == null)
                    return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

                string variableValue = ((IMonaVariablesStringValue)variable).Value;

                variableValue = variableValue.Replace(_replaceThis, _withThis);
                _brain.Variables.Set(_stringName, variableValue);

                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}