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
    public class GetStringLengthToInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "GetStringLength";
        public const string NAME = "Get String Length";
        public const string CATEGORY = "Strings";

        public override Type TileType => typeof(GetStringLengthToInstructionTile);

        public virtual ValueChangeType Operator => ValueChangeType.Set;

        [SerializeField] private string _stringName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string StringName { get => _stringName; set => _stringName = value; }

        [SerializeField] private string _numberToSet;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberTOSet { get => _numberToSet; set => _numberToSet = value; }

        protected IMonaBrain _brain;

        public GetStringLengthToInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_stringName) || string.IsNullOrEmpty(_numberToSet))
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            string stringValue = _brain.Variables.GetString(_stringName);
            float length = stringValue.Length;
            _brain.Variables.Set(_numberToSet, length);

            return Complete(InstructionTileResult.Success);
        }
    }
}