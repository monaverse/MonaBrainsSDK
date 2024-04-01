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
using System.Text.RegularExpressions;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class GetNumberFromStringInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "GetStringNumbers";
        public const string NAME = "Get String Numbers";
        public const string CATEGORY = "Strings";

        public override Type TileType => typeof(GetNumberFromStringInstructionTile);

        public virtual ValueChangeType Operator => ValueChangeType.Set;

        [SerializeField] private string _stringName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string StringName { get => _stringName; set => _stringName = value; }

        [SerializeField] private string _numberToSet;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberToSet { get => _numberToSet; set => _numberToSet = value; }

        [SerializeField] private NumberGetType _numbersToGet = NumberGetType.AllCombined;
        [BrainProperty(false)] public NumberGetType NumbersToGet { get => _numbersToGet; set => _numbersToGet = value; }

        [SerializeField]
        public enum NumberGetType
        {
            AllCombined = 0,
            FirstNumberSet = 10,
            AddNumberSets = 20
        }

        protected IMonaBrain _brain;

        public GetNumberFromStringInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_stringName) || string.IsNullOrEmpty(_numberToSet))
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            string stringValue = _brain.Variables.GetString(_stringName);

            return Evaluate(stringValue) ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(string stringValue)
        {
            float result;
            string allNumbersConcatenated = "";

            var matches = Regex.Matches(stringValue, @"-?\d+(\.\d+)?");

            for (int i = 0; i < matches.Count; i++)
                allNumbersConcatenated += matches[i].Value.Replace(".", "").Replace("-", "");

            if (string.IsNullOrEmpty(allNumbersConcatenated))
                return false;

            switch (_numbersToGet)
            {
                case NumberGetType.AllCombined:
                    double allNumbersDouble = double.Parse(allNumbersConcatenated);
                    result = (float)Math.Min(Math.Max(allNumbersDouble, float.MinValue), float.MaxValue);
                    break;
                case NumberGetType.FirstNumberSet:
                    if (matches.Count < 1)
                        return false;
                    double firstNumberDouble = double.Parse(matches[0].Value);
                    result = (float)Math.Min(Math.Max(firstNumberDouble, float.MinValue), float.MaxValue);
                    break;
                case NumberGetType.AddNumberSets:
                    double sum = 0;

                    for (int i = 0; i < matches.Count; i++)
                        sum += double.Parse(matches[i].Value);

                    result = (float)Math.Min(Math.Max(sum, float.MinValue), float.MaxValue);
                    break;
                default:
                    return false;
            }

            _brain.Variables.Set(_numberToSet, result);
            return true;
        }
    }
}