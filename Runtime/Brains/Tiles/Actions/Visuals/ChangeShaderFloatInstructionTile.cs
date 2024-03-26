using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{

    [Serializable]
    public class ChangeShaderFloatInstructionTile : ChangeShaderInstructionTile
    {
        public new const string ID = "ChangeShaderFloat";
        public new const string NAME = "Shader Float";
        public new const string CATEGORY = "Audio / Visual";
        public override Type TileType => typeof(ChangeShaderFloatInstructionTile);

        [SerializeField] private float _value = 0f;
        [SerializeField] private string _valueName;
        [BrainProperty(true)] public float Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueName { get => _valueName; set => _valueName = value; }

        public ChangeShaderFloatInstructionTile() { }

        private float _start;
        private float _end;

        protected override void SetValueProgress()
        {
            _brain.Body.SetShaderFloat(_propertyName, Mathf.Lerp(_start, _end, Evaluate(Progress)));
        }

        protected override void SetValueEnd()
        {
            if (!string.IsNullOrEmpty(_valueName))
                _value = _brain.Variables.GetFloat(_valueName);

            _brain.Body.SetShaderFloat(_propertyName, _value);
        }

        protected override void ResetValue()
        {
            if (!string.IsNullOrEmpty(_valueName))
                _value = _brain.Variables.GetFloat(_valueName);

            _start = _brain.Body.GetShaderFloat(_propertyName);
            _end = _value;
        }

    }
}