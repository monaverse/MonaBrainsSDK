using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{

    [Serializable]
    public class ChangeShaderVectorInstructionTile : ChangeShaderInstructionTile
    {
        public new const string ID = "ChangeShaderVector";
        public new const string NAME = "Shader Vector";
        public new const string CATEGORY = "Audio / Visual";
        public override Type TileType => typeof(ChangeShaderVectorInstructionTile);

        [SerializeField] private Vector3 _value;
        [SerializeField] private string[] _valueName;
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string[] ValueName { get => _valueName; set => _valueName = value; }

        public ChangeShaderVectorInstructionTile() { }

        private Vector4 _start;
        private Vector4 _end;

        protected override void SetValueProgress()
        {
            _brain.Body.SetShaderVector(_propertyName, Vector4.Lerp(_start, _end, Evaluate(Progress)));
        }

        protected override void SetValueEnd()
        {
            if (HasVector3Values(_valueName))
                _value = GetVector3Value(_brain, _valueName);

            _brain.Body.SetShaderVector(_propertyName, _value);
        }

        protected override void ResetValue()
        {
            if (HasVector3Values(_valueName))
                _value = GetVector3Value(_brain, _valueName);

            _start = _brain.Body.GetShaderVector(_propertyName);
            _end = _value;
        }

    }
}