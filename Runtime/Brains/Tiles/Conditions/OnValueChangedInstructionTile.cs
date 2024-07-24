using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnValueChangedInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile
    {
        public const string ID = "OnValueChanged";
        public const string NAME = "Value Has Changed";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(OnValueChangedInstructionTile);

        [SerializeField] private string _valueName;
        [BrainPropertyValue(true)] public string ValueName { get => _valueName; set => _valueName = value; }

        private IMonaBrain _brain;
        private float _lastFloat;
        private string _lastString;
        private bool _lastBool;
        private Vector2 _lastVector2;
        private Vector3 _lastVector3;
        private IMonaBrain _lastBrain;
        private IMonaBody _lastBody;

        public OnValueChangedInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            DefaultValue(_brain.Variables);
        }

        private void DefaultValue(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_valueName);
            if (variable is IMonaVariablesFloatValue)
                _lastFloat = ((IMonaVariablesFloatValue)variable).ValueToReturnFromTile;
            else if (variable is IMonaVariablesStringValue)
                _lastString = ((IMonaVariablesStringValue)variable).Value;
            else if (variable is IMonaVariablesBoolValue)
                _lastBool = ((IMonaVariablesBoolValue)variable).Value;
            else if (variable is IMonaVariablesVector2Value)
                _lastVector2 = ((IMonaVariablesVector2Value)variable).Value;
            else if (variable is IMonaVariablesVector3Value)
                _lastVector3 = ((IMonaVariablesVector3Value)variable).Value;
            else if (variable is IMonaVariablesBodyValue)
                _lastBody = ((IMonaVariablesBodyValue)variable).Value;
            else if (variable is IMonaVariablesBrainValue)
                _lastBrain = ((IMonaVariablesBrainValue)variable).Value;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && Evaluate(_brain.Variables))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_valueName);
            if(variable is IMonaVariablesFloatValue)
                return EvaluateValue((IMonaVariablesFloatValue)variable);
            else if (variable is IMonaVariablesStringValue)
                return EvaluateValue((IMonaVariablesStringValue)variable);
            else if (variable is IMonaVariablesBoolValue)
                return EvaluateValue((IMonaVariablesBoolValue)variable);
            else if (variable is IMonaVariablesVector2Value)
                return EvaluateValue((IMonaVariablesVector2Value)variable);
            else if (variable is IMonaVariablesVector3Value)
                return EvaluateValue((IMonaVariablesVector3Value)variable);
            else if (variable is IMonaVariablesBodyValue)
                return EvaluateValue((IMonaVariablesBodyValue)variable);
            else if (variable is IMonaVariablesBrainValue)
                return EvaluateValue((IMonaVariablesBrainValue)variable);
            return false;
        }

        private bool EvaluateValue(IMonaVariablesFloatValue value)
        {
            if (value.Value != _lastFloat)
            {
                _lastFloat = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesStringValue value)
        {
            if (value.Value != _lastString)
            {
                _lastString = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesBoolValue value)
        {
            if (value.Value != _lastBool)
            {
                _lastBool = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesVector2Value value)
        {
            if (value.Value != _lastVector2)
            {
                _lastVector2 = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesVector3Value value)
        {
            if (value.Value != _lastVector3)
            {
                _lastVector3 = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesBrainValue value)
        {
            if (value.Value != _lastBrain)
            {
                _lastBrain = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaVariablesBodyValue value)
        {
            if (value.Value != _lastBody)
            {
                _lastBody = value.Value;
                return true;
            }
            return false;
        }

    }
}