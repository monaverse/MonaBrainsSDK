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
    public class NumberSetRandomSeedInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, INeedAuthorityInstructionTile
    {
        public const string ID = "SetRandomSeed";
        public const string NAME = "Set Random Seed";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(NumberSetRandomSeedInstructionTile);

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }        

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private string _seedValue;
        [SerializeField] private string _seedValueName;
        [BrainProperty(true)] public string SeedValue { get => _seedValue; set => _seedValue = value; }
        [BrainPropertyValueName("SeedValue", typeof(IMonaVariablesStringValue))] public string SeedValueName { get => _seedValueName; set => _seedValueName = value; }

        [SerializeField] private RandomAttributeChangeType _changeType;
        [BrainPropertyEnum(false)] public RandomAttributeChangeType ChangeType { get => _changeType; set => _changeType = value; }

        [SerializeField] private float _minValue = 0;
        [SerializeField] private string _minValueName;
        [BrainPropertyShow(nameof(ChangeType), (int)RandomAttributeChangeType.ForceSeedAndSetRange)]
        [BrainProperty(false)] public float MinValue { get => _minValue; set => _minValue = value; }
        [BrainPropertyValueName("MinValue", typeof(IMonaVariablesFloatValue))] public string MinValueName { get => _minValueName; set => _minValueName = value; }

        [SerializeField] private float _maxValue = 1;
        [SerializeField] private string _maxValueName;
        [BrainPropertyShow(nameof(ChangeType), (int)RandomAttributeChangeType.ForceSeedAndSetRange)]
        [BrainProperty(false)] public float MaxValue { get => _maxValue; set => _maxValue = value; }
        [BrainPropertyValueName("MaxValue", typeof(IMonaVariablesFloatValue))] public string MaxValueName { get => _maxValueName; set => _maxValueName = value; }

        [SerializeField] private bool _forceRandomReset;
        [SerializeField] private string _forceRandomResetName;
        [BrainProperty(false)] public bool ForceRandomReset { get => _forceRandomReset; set => _forceRandomReset = value; }
        [BrainPropertyValueName("ForceRandomReset", typeof(IMonaVariablesBoolValue))] public string ForceRandomResetName { get => _forceRandomResetName; set => _forceRandomResetName = value; }

        private IMonaBrain _brain;

        public enum RandomAttributeChangeType
        {
            DoNotChange = 0,
            ForceRandomWithSeed = 10,
            ForceSeedAndSetRange = 20
        }

        public NumberSetRandomSeedInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_numberName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_seedValueName))
                _seedValue = _brain.Variables.GetString(_seedValueName);

            if (!string.IsNullOrEmpty(_minValueName))
                _minValue = _brain.Variables.GetFloat(_minValueName);

            if (!string.IsNullOrEmpty(_maxValueName))
                _maxValue = _brain.Variables.GetFloat(_maxValueName);

            if (!string.IsNullOrEmpty(_forceRandomResetName))
                _forceRandomReset = _brain.Variables.GetBool(_forceRandomResetName);

            var variable = (IMonaVariablesFloatValue)_brain.Variables.GetVariable(_numberName);

            if (_changeType != RandomAttributeChangeType.DoNotChange)
            {
                if (variable.MinMaxType == MinMaxConstraintType.None)
                    variable.MinMaxType = MinMaxConstraintType.ConstrainToBounds;

                if (_changeType == RandomAttributeChangeType.ForceSeedAndSetRange)
                    variable.ForceMinMax(_minValue, _maxValue);

                variable.ReturnRandomValueFromMinMax = true;
                variable.UseRandomSeed = true;
            }

            if (variable.RandomSeed != _seedValue)
                variable.RandomSeed = _seedValue;

            if (_forceRandomReset)
                variable.ResetRandom();

            return Complete(InstructionTileResult.Success);
        }
    }
}