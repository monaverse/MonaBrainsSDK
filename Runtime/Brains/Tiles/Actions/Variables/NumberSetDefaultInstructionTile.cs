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
    public class NumberSetDefaultInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile

    {
        public const string ID = "SetDefault";
        public const string NAME = "Set Default";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(NumberSetDefaultInstructionTile);

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private float _newDefault;
        [SerializeField] private string _newDefaultName;
        [BrainProperty(true)] public float NewDefault { get => _newDefault; set => _newDefault = value; }
        [BrainPropertyValueName("NewDefault", typeof(IMonaVariablesFloatValue))] public string NewDefaultName { get => _newDefaultName; set => _newDefaultName = value; }

        private IMonaBrain _brain;

        public NumberSetDefaultInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_numberName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_newDefaultName))
                _newDefault = _brain.Variables.GetFloat(_newDefaultName);

            var variable = (IMonaVariablesFloatValue)_brain.Variables.GetVariable(_numberName);

            variable.DefaultValue = _newDefault;

            return Complete(InstructionTileResult.Success);
        }
    }
}