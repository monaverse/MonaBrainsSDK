using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SetNumberToMinInstructionTile : InstructionTile, ISetNumberToConstraintInstructionTile, IActionInstructionTile
    {
        public const string ID = "SetNumberToMin";
        public const string NAME = "Set Number To Min";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetNumberToMinInstructionTile);

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        private IMonaBrain _brain;

        public SetNumberToMinInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null)
            {
                var floatVariable = (IMonaVariablesFloatValue)_brain.Variables.GetVariable(_numberName);
                _brain.Variables.Set(_numberName, floatVariable.Min);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}