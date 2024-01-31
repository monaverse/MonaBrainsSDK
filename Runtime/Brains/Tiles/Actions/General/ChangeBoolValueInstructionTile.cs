﻿using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Tiles.Actions.General.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeBoolValueInstructionTile : InstructionTile, IChangeBoolValueInstructionTile, IActionInstructionTile
    {
        public const string ID = "ChangeBoolValue";
        public const string NAME = "Change Bool Value";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(ChangeBoolValueInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaStateBoolValue), true)] public string ValueName { get => _valueName; set => _valueName = value; }

        [SerializeField] private bool _value;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public bool Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value")] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;

        public ChangeBoolValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.State.GetBool(_valueValueName);

            if (_brain != null)
            {
                if(Evaluate(_brain.State))
                    return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainState state)
        {
            var value = state.GetValue(_valueName);
            if (value == null)
                return true;

            if (value is IMonaStateBoolValue)
                ChangeBoolValue(state, _valueName, _value);
            return true;
        }

        private void ChangeBoolValue(IMonaBrainState state, string name, bool value)
        {
            state.Set(name, value);
        }
    }
}