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
using Mona.SDK.Core.Body;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class NumberResetRandomInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, INeedAuthorityInstructionTile
    {
        public const string ID = "ResetRandomSeed";
        public const string NAME = "Reset Random Seed";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(NumberResetRandomInstructionTile);

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }        

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        private IMonaBrain _brain;

        public enum RandomAttributeChangeType
        {
            DoNotChange = 0,
            ForceRandomWithSeed = 10,
            ForceSeedAndSetRange = 20
        }

        public NumberResetRandomInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_numberName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            ((IMonaVariablesFloatValue)_brain.Variables.GetVariable(_numberName)).ResetRandom();

            return Complete(InstructionTileResult.Success);
        }
    }
}