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
    public class OnValueChangedInstructionTile : InstructionTile, IInstructionTileWithPreload, IOnValueChangedInstructionTile, IConditionInstructionTile, IStartableInstructionTile
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
            DefaultValue(_brain.State);
        }

        private void DefaultValue(IMonaBrainState state)
        {
            var value = state.GetValue(_valueName);
            if (value is IMonaStateFloatValue)
                _lastFloat = ((IMonaStateFloatValue)value).Value;
            else if (value is IMonaStateStringValue)
                _lastString = ((IMonaStateStringValue)value).Value;
            else if (value is IMonaStateBoolValue)
                _lastBool = ((IMonaStateBoolValue)value).Value;
            else if (value is IMonaStateVector2Value)
                _lastVector2 = ((IMonaStateVector2Value)value).Value;
            else if (value is IMonaStateVector3Value)
                _lastVector3 = ((IMonaStateVector3Value)value).Value;
            else if (value is IMonaStateBodyValue)
                _lastBody = ((IMonaStateBodyValue)value).Value;
            else if (value is IMonaStateBrainValue)
                _lastBrain = ((IMonaStateBrainValue)value).Value;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && Evaluate(_brain.State))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool Evaluate(IMonaBrainState state)
        {
            var value = state.GetValue(_valueName);
            if(value is IMonaStateFloatValue)
                return EvaluateValue((IMonaStateFloatValue)value);
            else if (value is IMonaStateStringValue)
                return EvaluateValue((IMonaStateStringValue)value);
            else if (value is IMonaStateBoolValue)
                return EvaluateValue((IMonaStateBoolValue)value);
            else if (value is IMonaStateVector2Value)
                return EvaluateValue((IMonaStateVector2Value)value);
            else if (value is IMonaStateVector3Value)
                return EvaluateValue((IMonaStateVector3Value)value);
            else if (value is IMonaStateBodyValue)
                return EvaluateValue((IMonaStateBodyValue)value);
            else if (value is IMonaStateBrainValue)
                return EvaluateValue((IMonaStateBrainValue)value);
            return false;
        }

        private bool EvaluateValue(IMonaStateFloatValue value)
        {
            if (value.Value != _lastFloat)
            {
                _lastFloat = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateStringValue value)
        {
            if (value.Value != _lastString)
            {
                _lastString = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateBoolValue value)
        {
            if (value.Value != _lastBool)
            {
                _lastBool = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateVector2Value value)
        {
            if (value.Value != _lastVector2)
            {
                _lastVector2 = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateVector3Value value)
        {
            if (value.Value != _lastVector3)
            {
                _lastVector3 = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateBrainValue value)
        {
            if (value.Value != _lastBrain)
            {
                _lastBrain = value.Value;
                return true;
            }
            return false;
        }

        private bool EvaluateValue(IMonaStateBodyValue value)
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