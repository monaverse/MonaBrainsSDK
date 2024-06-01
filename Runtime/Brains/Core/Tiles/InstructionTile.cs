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
        public event Action<InstructionTileResult, string, IInstructionTile> OnExecute = delegate { };
        public event Action OnMuteChanged = delegate { };

        [HideInInspector][SerializeField] private string _id;
        public string Id { get => _id; set => _id = value; }

        [HideInInspector][SerializeField] private string _name;
        public string Name { get => _name; set => _name = value; }

        [HideInInspector][SerializeField] private string _category;
        public string Category { get => _category; set => _category = value; }

        [HideInInspector] [SerializeField] private bool _muted;
        public bool Muted {
            get {
               return _muted;
            }
            set {
                _muted = value;
                OnMuteChanged?.Invoke();
            }
        }

        public abstract Type TileType { get; }

        public InstructionTileResult LastResult { get; set; }

        protected IInstructionTile _nextTile;
        public IInstructionTile NextExecutionTile { get => _nextTile; set => _nextTile = value; }

        protected InstructionTileCallback _thenCallback = new InstructionTileCallback();
        public InstructionTileCallback ThenCallback => _thenCallback;
        
        public virtual void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _thenCallback.Tile = tile;
                _thenCallback.ActionCallback = thenCallback;
            }
        }
        
        public InstructionTileResult Complete(InstructionTileResult result, bool invokeCallback)
        {
            if (LastResult != result)
                TriggerOnExecute(result, null);

            if (result == InstructionTileResult.LostAuthority)
                NextExecutionTile = null;

            if (invokeCallback && _thenCallback.ActionCallback != null)
                return _thenCallback.ActionCallback(_thenCallback);

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

        public virtual void Unload(bool destroy = false) { }

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
            var vector2 = new Vector2();
            if (!string.IsNullOrEmpty(values[0]))
                vector2 = brain.Variables.GetVector2(values[0]);

            if (!string.IsNullOrEmpty(values[1]))
                vector2.x = brain.Variables.GetFloat(values[1]);

            if (!string.IsNullOrEmpty(values[2]))
                vector2.y = brain.Variables.GetFloat(values[2]);

            return vector2;
        }

        protected IInstruction _instruction;
        protected bool _firstTile;
        protected List<IMonaBody> _bodies = new List<IMonaBody>();
        protected void FilterBodiesOnInstruction(List<ForwardBodyStruct> bodies)
        {
            _bodies = _instruction.InstructionBodies;

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
                        _bodies.RemoveAt(i);
                }
            }

            //Debug.Log($"{nameof(FilterBodiesOnInstruction)} index {_instruction.InstructionTiles.IndexOf(this)} {_bodies.Count}");
            //_instruction.InstructionBodies = _bodies;
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

            Debug.Log($"{nameof(FilterBodiesOnInstruction)} index {_instruction.InstructionTiles.IndexOf(this)} {_bodies.Count}");
            _instruction.InstructionBodies = _bodies;
        }

    }
}