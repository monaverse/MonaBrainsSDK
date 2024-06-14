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

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class NumberSetMinMaxInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, INeedAuthorityInstructionTile

    {
        public const string ID = "SetMinMax";
        public const string NAME = "Set Min / Max";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(NumberSetMinMaxInstructionTile);

        public IMonaBody GetBodyToControl() => _brain.Body;

        [SerializeField] private ValuesToSetType _valuesToSet;
        [BrainPropertyEnum(false)] public ValuesToSetType ValuesToSet { get => _valuesToSet; set => _valuesToSet = value; }

        [SerializeField] private MinMaxChangeType _changeType;
        [BrainPropertyEnum(false)] public MinMaxChangeType ChangeType { get => _changeType; set => _changeType = value; }

        [SerializeField] private MinMaxConstraintType _minMaxType;
        [BrainPropertyShow(nameof(ChangeType), (int)MinMaxChangeType.UpdateType)]
        [BrainPropertyEnum(false)] public MinMaxConstraintType MinMaxType { get => _minMaxType; set => _minMaxType = value; }

        [SerializeField] private string _numberName;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private float _minValue = 0;
        [SerializeField] private string _minValueName;
        [BrainPropertyShow(nameof(ValuesToSet), (int)ValuesToSetType.MinAndMax)]
        [BrainPropertyShow(nameof(ValuesToSet), (int)ValuesToSetType.MinOnly)]
        [BrainProperty(true)] public float MinValue { get => _minValue; set => _minValue = value; }
        [BrainPropertyValueName("MinValue", typeof(IMonaVariablesFloatValue))] public string MinValueName { get => _minValueName; set => _minValueName = value; }

        [SerializeField] private float _maxValue = 10;
        [SerializeField] private string _maxValueName;
        [BrainPropertyShow(nameof(ValuesToSet), (int)ValuesToSetType.MinAndMax)]
        [BrainPropertyShow(nameof(ValuesToSet), (int)ValuesToSetType.MaxOnly)]
        [BrainProperty(true)] public float MaxValue { get => _maxValue; set => _maxValue = value; }
        [BrainPropertyValueName("MaxValue", typeof(IMonaVariablesFloatValue))] public string MaxValueName { get => _maxValueName; set => _maxValueName = value; }

        private IMonaBrain _brain;

        public enum ValuesToSetType
        {
            MinAndMax,
            MinOnly,
            MaxOnly
        }

        public enum MinMaxChangeType
        {
            DoNotChange,
            UpdateType
        }

        public NumberSetMinMaxInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_numberName))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_minValueName))
                _minValue = _brain.Variables.GetFloat(_minValueName);

            if (!string.IsNullOrEmpty(_maxValueName))
                _maxValue = _brain.Variables.GetFloat(_maxValueName);

            var variable = (IMonaVariablesFloatValue)_brain.Variables.GetVariable(_numberName);

            if (_changeType == MinMaxChangeType.UpdateType)
                variable.MinMaxType = _minMaxType;

            switch (_valuesToSet)
            {
                case ValuesToSetType.MinAndMax:
                    variable.ForceMinMax(_minValue, _maxValue);
                    break;
                case ValuesToSetType.MinOnly:
                    if (_minValue <= variable.Max)
                        variable.Min = _minValue;
                    break;
                case ValuesToSetType.MaxOnly:
                    if (_maxValue >= variable.Min)
                        variable.Max = _maxValue;
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}