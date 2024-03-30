using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using UnityEngine;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    public class StringAddInstructionTile : SetStringToInstructionTile
    {
        public override Type TileType => typeof(StringAddInstructionTile);

        [SerializeField] private InsertLocationType _placement = InsertLocationType.EndOfString;
        [BrainProperty(false)] public InsertLocationType Placement { get => _placement; set => _placement = value; }

        [SerializeField] private float _index = 0;
        [SerializeField] private string _indexName;
        [BrainPropertyShow(nameof(Placement), (int)InsertLocationType.AtIndex)]
        [BrainProperty(false)] public float Index { get => _index; set => _index = value; }
        [BrainPropertyValueName("Index", typeof(IMonaVariablesFloatValue))] public string IndexName { get => _indexName; set => _indexName = value; }

        [SerializeField] private SpacingType _spacing = SpacingType.None;
        [BrainProperty(false)] public SpacingType Spacing { get => _spacing; set => _spacing = value; }

        protected const string spacer = " ";

        protected string PrefixSpacer => _spacing == SpacingType.BeforeValue || _spacing == SpacingType.BeforeAndAfterValue ?
            spacer : string.Empty;

        protected string SuffixSpacer => _spacing == SpacingType.AfterValue || _spacing == SpacingType.BeforeAndAfterValue ?
            spacer : string.Empty;

        protected string ValueWithSpacers => PrefixSpacer + _value + SuffixSpacer;

        [Serializable]
        public enum InsertLocationType
        {
            EndOfString,
            StartOfString,
            AtIndex
        }

        [Serializable]
        public enum SpacingType
        {
            None,
            BeforeValue,
            AfterValue,
            BeforeAndAfterValue
        }

        protected override bool Evaluate(IMonaBrainVariables state)
        {
            if (!string.IsNullOrEmpty(_indexName))
                _index = _brain.Variables.GetFloat(_indexName);

            var variable = state.GetVariable(_stringName);

            if (variable == null)
            {
                state.Set(_stringName, _value);
                return true;
            }

            if (!(variable is IMonaVariablesStringValue))
                return false;

            string variableValue = ((IMonaVariablesStringValue)variable).Value;

            switch (_placement)
            {
                case InsertLocationType.StartOfString:
                    state.Set(_stringName, variableValue.Insert(0, ValueWithSpacers));
                    break;
                case InsertLocationType.AtIndex:
                    int length = variableValue.Length;
                    int index = (int)_index;

                    if (index < 0)
                    {
                        index = 0;
                    }
                    else if (length > 0 && _index > length - 1)
                    {
                        index = length - 1;
                    }

                    state.Set(_stringName, variableValue.Insert(index, ValueWithSpacers));
                    break;
                default:
                    variableValue += ValueWithSpacers;
                    state.Set(_stringName, variableValue);
                    break;
            }

            return true;
        }
    }
}
