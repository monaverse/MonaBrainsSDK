using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{

    [Serializable]
    public class ChangeShaderVectorByTagInstructionTile : ChangeShaderInstructionTile
    {
        public new const string ID = "ChangeShaderVectors";
        public new const string NAME = "Shader Vectors";
        public new const string CATEGORY = "Audio / Visual";
        public override Type TileType => typeof(ChangeShaderVectorByTagInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private MonaBodyValueType _source = MonaBodyValueType.Position;
        [BrainPropertyEnum(true)] public MonaBodyValueType Source { get => _source; set => _source = value; }

        public ChangeShaderVectorByTagInstructionTile() { }

        private Vector4[] _start;
        private Vector4[] _end;
        private Vector4[] _lerp;

        protected override void SetValueProgress()
        {
            var p = Evaluate(Progress);

            for (var i = 0; i < _start.Length; i++)
                _lerp[i] = Vector4.Lerp(_start[i], _end[i], p);

            Shader.SetGlobalVectorArray(_propertyName, _lerp);
        }

        protected override void SetValueEnd()
        {
            var bodies = MonaBody.FindByTag(_tag);
            if (_end == null || bodies.Count != _end.Length)
                _end = new Vector4[bodies.Count];

            for (var i = 0; i < bodies.Count; i++)
            {
                switch(_source)
                {
                    case MonaBodyValueType.Position:
                        _end[i] = bodies[i].GetPosition(); break;
                    case MonaBodyValueType.Rotation:
                        _end[i] = bodies[i].GetRotation().eulerAngles / 360f; break;
                    case MonaBodyValueType.Scale:
                        _end[i] = bodies[i].GetScale(); break;
                    case MonaBodyValueType.Velocity:
                        _end[i] = bodies[i].GetVelocity(); break;
                }
                //Debug.Log($"{_propertyName} {i} {_end[i]} {Time.frameCount}");
            }

            Shader.SetGlobalVectorArray(_propertyName, _end);
        }

        protected override void ResetValue()
        {
            var bodies = MonaBody.FindByTag(_tag);

            _start = new Vector4[bodies.Count];
            _end = new Vector4[bodies.Count];
            _lerp = new Vector4[bodies.Count];

            var arr = Shader.GetGlobalVectorArray(_propertyName);
            if(arr == null || arr.Length != bodies.Count)
            {
                Shader.SetGlobalVectorArray(_propertyName, new Vector4[bodies.Count]);
                arr = Shader.GetGlobalVectorArray(_propertyName);
            }

            for (var i = 0;i < bodies.Count; i++)
            {
                _start[i] = arr[i];
                switch (_source)
                {
                    case MonaBodyValueType.Position:
                        _end[i] = bodies[i].GetPosition(); break;
                    case MonaBodyValueType.Rotation:
                        _end[i] = bodies[i].GetRotation().eulerAngles / 360f; break;
                    case MonaBodyValueType.Scale:
                        _end[i] = bodies[i].GetScale(); break;
                    case MonaBodyValueType.Velocity:
                        _end[i] = bodies[i].GetVelocity(); break;
                }
            }
        }

    }
}