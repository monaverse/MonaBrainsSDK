using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{

    [Serializable]
    public class ChangeShaderColorInstructionTile : ChangeShaderInstructionTile
    {
        public new const string ID = "ChangeShaderColor";
        public new const string NAME = "Shader Color";
        public new const string CATEGORY = "Audio / Visual";
        public override Type TileType => typeof(ChangeShaderColorInstructionTile);

        [SerializeField] private Color _value = Color.white;
        [BrainProperty(true)] public Color Value { get => _value; set => _value = value; }

        public ChangeShaderColorInstructionTile() { }

        private Color _start;
        private Color _end;

        protected override void SetValueProgress()
        {
            _brain.Body.SetShaderColor(_propertyName, Color.Lerp(_start, _end, Evaluate(Progress)));
        }

        protected override void SetValueEnd()
        {
            _brain.Body.SetShaderColor(_propertyName, _value);
        }

        protected override void ResetValue()
        {
            _start = _brain.Body.GetShaderColor(_propertyName);
            _end = _value;
        }

    }
}