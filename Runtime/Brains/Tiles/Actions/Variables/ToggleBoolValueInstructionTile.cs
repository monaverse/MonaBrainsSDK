using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class ToggleBoolValueInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IOnStartInstructionTile
    {
        public const string ID = "ToggleBoolValue";
        public const string NAME = "Toggle Bool Value";
        public const string CATEGORY = "Booleans";
        public override Type TileType => typeof(ToggleBoolValueInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        private IMonaBrain _brain;

        public ToggleBoolValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var value = _brain.Variables.GetVariable(_valueName);

            if (value == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            bool currentValue = ((IMonaVariablesBoolValue)value).Value;
            _brain.Variables.Set(_valueName, !currentValue);

            return Complete(InstructionTileResult.Success);
        }
    }
}