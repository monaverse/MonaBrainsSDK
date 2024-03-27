using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Structs;
using Mona.SDK.Core.Body;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    [Serializable]
    public abstract class InstructionTile : IInstructionTile
    {
        public event Action<InstructionTileResult, string, IInstructionTile> OnExecute;

        [HideInInspector][SerializeField] private string _id;
        public string Id { get => _id; set => _id = value; }

        [HideInInspector][SerializeField] private string _name;
        public string Name { get => _name; set => _name = value; }

        [HideInInspector][SerializeField] private string _category;
        public string Category { get => _category; set => _category = value; }

        public abstract Type TileType { get; }

        public InstructionTileResult LastResult { get; set; }

        protected IInstructionTile _nextTile;
        public IInstructionTile NextExecutionTile { get => _nextTile; set => _nextTile = value; }

        protected IInstructionTileCallback _thenCallback;
        public IInstructionTileCallback ThenCallback => _thenCallback;
        public virtual void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
                _thenCallback = thenCallback;
        }
        
        public InstructionTileResult Complete(InstructionTileResult result, bool invokeCallback)
        {
            if (LastResult != result)
                TriggerOnExecute(result, null);

            if (result == InstructionTileResult.LostAuthority)
                NextExecutionTile = null;

            if (invokeCallback && _thenCallback != null)
                return _thenCallback.Action();

            return result;
        }

        public InstructionTileResult Complete(InstructionTileResult result, string reason = null)
        {
            if(LastResult != result)
                TriggerOnExecute(result, reason);

            //Debug.Log($"{nameof(InstructionTile)} {nameof(Complete)} Name: {Name} Result: {result}");
            return result;
        }

        protected void TriggerOnExecute(InstructionTileResult result, string reason)
        {
            LastResult = result;
            OnExecute?.Invoke(result, null, this);
        }

        public abstract InstructionTileResult Do();

        public virtual void Unload() { }

        protected bool HasVector3Values(string[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                    return true;
            }
            return false;
        }

        protected Vector3 GetVector3Value(IMonaBrain brain, string[] values)
        {
            var vector3 = new Vector3();
            if (!string.IsNullOrEmpty(values[0]))
                vector3 = brain.Variables.GetVector3(values[0]);

            if (!string.IsNullOrEmpty(values[1]))
                vector3.x = brain.Variables.GetFloat(values[1]);

            if (!string.IsNullOrEmpty(values[2]))
                vector3.y = brain.Variables.GetFloat(values[2]);

            if (!string.IsNullOrEmpty(values[3]))
                vector3.z = brain.Variables.GetFloat(values[3]);

            return vector3;
        }

        protected bool HasVector2Values(string[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                    return true;
            }
            return false;
        }

        protected Vector3 GetVector2Value(IMonaBrain brain, string[] values)
        {
            var vector3 = new Vector3();
            if (!string.IsNullOrEmpty(values[0]))
                vector3 = brain.Variables.GetVector3(values[0]);

            if (!string.IsNullOrEmpty(values[1]))
                vector3.x = brain.Variables.GetFloat(values[1]);

            if (!string.IsNullOrEmpty(values[2]))
                vector3.y = brain.Variables.GetFloat(values[2]);

            return vector3;
        }

        protected IInstruction _instruction;
        protected bool _firstTile;
        protected List<IMonaBody> _bodies = new List<IMonaBody>();
        protected void FilterBodiesOnInstruction(List<ForwardBodyStruct> bodies)
        {
            if (_firstTile)
            {
                _bodies.Clear();
                for (var i = 0; i < bodies.Count; i++)
                    _bodies.Add(bodies[i].body);
            }
            else
            {
                for (var i = _bodies.Count - 1; i >= 0; i--)
                {
                    var found = false;
                    for (var j = 0; j < bodies.Count; j++)
                    {
                        if (bodies[j].body == _bodies[i])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        bodies.RemoveAt(i);
                }
            }

            //Debug.Log($"{nameof(FilterBodiesOnInstruction)} index {_instruction.InstructionTiles.IndexOf(this)} {_bodies.Count}");
            _instruction.InstructionBodies = _bodies;
        }

        protected void FilterBodiesOnInstruction(List<IMonaBody> bodies)
        {
            if (_firstTile)
            {
                _bodies.Clear();
                for (var i = 0; i < bodies.Count; i++)
                    _bodies.Add(bodies[i]);
            }
            else
            {
                for (var i = _bodies.Count - 1; i >= 0; i--)
                {
                    var found = false;
                    for (var j = 0; j < bodies.Count; j++)
                    {
                        if (bodies[j] == _bodies[i])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        bodies.RemoveAt(i);
                }
            }

            //Debug.Log($"{nameof(FilterBodiesOnInstruction)} index {_instruction.InstructionTiles.IndexOf(this)} {_bodies.Count}");
            _instruction.InstructionBodies = _bodies;
        }

    }
}