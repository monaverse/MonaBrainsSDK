using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.EasyUI;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class ToggleVariableUIDisplayInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "ToggleVariableUI";
        public const string NAME = "Toggle UI";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(ToggleVariableUIDisplayInstructionTile);

        [SerializeField] private string _myVariable;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

        [SerializeField] private bool _displayInUI = true;
        [SerializeField] private string _displayInUIName;
        [BrainProperty(true)] public bool DisplayInUI { get => _displayInUI; set => _displayInUI = value; }
        [BrainPropertyValueName("DisplayInUI", typeof(IMonaVariablesBoolValue))] public string DisplayInUIName { get => _displayInUIName; set => _displayInUIName = value; }

        private IMonaBrain _brain;

        public ToggleVariableUIDisplayInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;


        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_myVariable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var myValue = _brain.Variables.GetVariable(_myVariable);

            if (!string.IsNullOrEmpty(_displayInUIName))
                _displayInUI = _brain.Variables.GetBool(_displayInUIName);

            if (myValue is IMonaVariablesFloatValue && ((IEasyUINumericalDisplay)myValue).AllowUIDisplay)
                ((IEasyUINumericalDisplay)myValue).DisplayInUI = _displayInUI;

            return Complete(InstructionTileResult.Success);
        }
    }
}