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
    public class StringClearInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "StringClear";
        public const string NAME = "Clear String";
        public const string CATEGORY = "Strings";
        public override Type TileType => typeof(StringClearInstructionTile);

        [SerializeField] private string _stringName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string StringName { get => _stringName; set => _stringName = value; }

        private IMonaBrain _brain;

        public StringClearInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (string.IsNullOrEmpty(_stringName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (_brain != null)
            {
                _brain.Variables.Set(_stringName, string.Empty);

                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}