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
    public class Vector3ModifyInstructionTile : InstructionTile, IActionInstructionTile, IStoreVariableInstructionTile

    {
        public const string ID = "ModifyVector3";
        public const string NAME = "Modify Vector3";
        public const string CATEGORY = "Vector3";
        public override Type TileType => typeof(Vector3ModifyInstructionTile);

        [SerializeField] private VariableTargetToStoreResult _setResultTo = VariableTargetToStoreResult.SameVariable;
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Add)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Subtract)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Multiply)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Divide)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Normalize)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Reflect)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Invert)]
        [BrainPropertyEnum(false)] public VariableTargetToStoreResult SetResultTo
        {
            get => Operation == VectorChangeType.Set ? VariableTargetToStoreResult.SameVariable : _setResultTo;
            set => _setResultTo = value;
        }

        [SerializeField] private VectorChangeType _operation;
        [BrainPropertyEnum(true)] public VectorChangeType Operation { get => _operation; set => _operation = value; }

        [SerializeField] private string _vector3Name;
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string Vector3Name { get => _vector3Name; set => _vector3Name = value; }

        [SerializeField] private Vector3 _amount;
        [SerializeField] private string[] _amountValueName;
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Set)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Add)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Subtract)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Multiply)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Divide)]
        [BrainPropertyShow(nameof(Operation), (int)VectorChangeType.Reflect)]
        [BrainProperty(true)] public Vector3 Amount { get => _amount; set => _amount = value; }
        [BrainPropertyValueName("Amount", typeof(IMonaVariablesVector3Value))] public string[] AmountValueName { get => _amountValueName; set => _amountValueName = value; }

        [SerializeField] private string _storeResultOn;
        [BrainPropertyShow(nameof(SetResultTo), (int)VariableTargetToStoreResult.OtherVariable)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string StoreResultOn { get => _storeResultOn; set => _storeResultOn = value; }

        private IMonaBrain _brain;

        public Vector3ModifyInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_vector3Name) || (SetResultTo == VariableTargetToStoreResult.OtherVariable && string.IsNullOrEmpty(_storeResultOn)))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (HasVector3Values(_amountValueName))
                _amount = GetVector3Value(_brain, _amountValueName);

            string nameOfVariableToSet = SetResultTo == VariableTargetToStoreResult.SameVariable ?
                _vector3Name : _storeResultOn;

            var variable = (IMonaVariablesVector3Value)_brain.Variables.GetVariable(_vector3Name);
            Vector3 value = variable.Value;

            switch (_operation)
            {
                case VectorChangeType.Set:
                    _brain.Variables.Set(nameOfVariableToSet, _amount);
                    break;
                case VectorChangeType.Add:
                    _brain.Variables.Set(nameOfVariableToSet, value + _amount);
                    break;
                case VectorChangeType.Subtract:
                    _brain.Variables.Set(nameOfVariableToSet, value - _amount);
                    break;
                case VectorChangeType.Multiply:
                    _brain.Variables.Set(nameOfVariableToSet, Vector3.Scale(value, _amount));
                    break;
                case VectorChangeType.Divide:
                    float newX = _amount.x != 0 ? value.x / _amount.x : value.x;
                    float newY = _amount.y != 0 ? value.y / _amount.y : value.y;
                    float newZ = _amount.z != 0 ? value.z / _amount.z : value.z;
                    _brain.Variables.Set(nameOfVariableToSet, new Vector3(newX, newY, newZ));
                    break;
                case VectorChangeType.Normalize:
                    _brain.Variables.Set(nameOfVariableToSet, Vector3.Normalize(value));
                    break;
                case VectorChangeType.Reflect:
                    _brain.Variables.Set(nameOfVariableToSet, Vector3.Reflect(value, _amount));
                    break;
                case VectorChangeType.Invert:
                    _brain.Variables.Set(nameOfVariableToSet, value * -1f);
                    break;
                case VectorChangeType.RoundClosest:
                    value.x = Mathf.Round(value.x);
                    value.y = Mathf.Round(value.y);
                    value.z = Mathf.Round(value.z);
                    _brain.Variables.Set(nameOfVariableToSet, value);
                    break;
                case VectorChangeType.RoundUp:
                    value.x = Mathf.Ceil(value.x);
                    value.y = Mathf.Ceil(value.y);
                    value.z = Mathf.Ceil(value.z);
                    _brain.Variables.Set(nameOfVariableToSet, value);
                    break;
                case VectorChangeType.RoundDown:
                    value.x = Mathf.Floor(value.x);
                    value.y = Mathf.Floor(value.y);
                    value.z = Mathf.Floor(value.z);
                    _brain.Variables.Set(nameOfVariableToSet, value);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}