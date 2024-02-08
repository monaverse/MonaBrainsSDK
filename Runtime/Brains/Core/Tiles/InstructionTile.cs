using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using System;
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
    }
}