using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Control
{
    [Serializable]
    public class Instruction : IInstruction
    {
        public event Action<IInstruction> OnReset = delegate { };

        [SerializeReference]
        private List<IInstructionTile> _instructionTiles = new List<IInstructionTile>();
        public List<IInstructionTile> InstructionTiles => _instructionTiles;

        [SerializeField]
        private InstructionTileResult _result = InstructionTileResult.Success;
        public InstructionTileResult Result { get => _result; set => _result = value; }

        public bool IsRunning() => _result == InstructionTileResult.Running;

        private int _firstActionIndex = 0;

        public Instruction()
        {
        }

        public void Preload(IMonaBrain brain)
        {
            _firstActionIndex = -1;
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];
                if (tile is IInstructionTileWithPreload)
                    ((IInstructionTileWithPreload)tile).Preload(brain);

                if (tile is IActionInstructionTile)
                {
                    if(_firstActionIndex == -1)
                        _firstActionIndex = i;
                    PreloadActionTile((IInstructionTile)tile);
                }

                if (i < InstructionTiles.Count - 1)
                    tile.NextExecutionTile = InstructionTiles[i + 1];
            } 
        }

        private void PreloadActionTile(IInstructionTile tile)
        {
            var callback = new InstructionTileCallback();
            callback.Action = () =>
            {
                //Debug.Log($"Execute Next from Then callback {tile}");
                return ExecuteActionTile(tile.NextExecutionTile);
            };
            tile.SetThenCallback(callback);
        }

        public void Execute(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            if (IsRunning()) return;

            if (_instructionTiles.Count == 0)
                return;
            
            if (ExecuteFirstTile(eventType, evt) == InstructionTileResult.Success)
            {
                OnReset?.Invoke(this);
                if (ExecuteRemainingConditionals() == InstructionTileResult.Success)
                    ExecuteActions();
            }
        }

        private InstructionTileResult ExecuteFirstTile(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            var tile = InstructionTiles[0];
            if (tile is IConditionInstructionTile)
            {
                switch (eventType)
                {
                    case InstructionEventTypes.Start:
                    case InstructionEventTypes.State:
                        if (tile is IOnStartInstructionTile)
                            return tile.Do();
                        break;
                    case InstructionEventTypes.Message:
                        if (tile is IOnMessageInstructionTile)
                            return tile.Do();
                        break;
                    case InstructionEventTypes.Value:
                        if (tile is IOnValueChangedInstructionTile)
                            return tile.Do();
                        break;
                    case InstructionEventTypes.Input:
                        if (tile is IInputInstructionTile)
                            return tile.Do();
                        break;
                    case InstructionEventTypes.Trigger:
                        if (tile is ITriggerInstructionTile && IsValidTriggerType((ITriggerInstructionTile)tile, (MonaTriggerEvent)evt))
                            return tile.Do();
                        break;
                }
            }
            return InstructionTileResult.Failure;
        }

        private bool IsValidTriggerType(ITriggerInstructionTile tile, MonaTriggerEvent evt)
        {
            return tile.TriggerTypes.Contains(evt.Type);
        }

        private InstructionTileResult ExecuteRemainingConditionals()
        {
            for(var i = 1;i < InstructionTiles.Count;i++)
            {
                var tile = InstructionTiles[i];
                if (tile is IConditionInstructionTile)
                {
                    if (tile.Do() == InstructionTileResult.Failure)
                        return InstructionTileResult.Failure;
                }
            }
            return InstructionTileResult.Success;
        }

        private void ExecuteActions()
        {
            _result = InstructionTileResult.Running;
            var tile = InstructionTiles[_firstActionIndex];
            //Debug.Log($"{nameof(ExecuteActions)} starting");
            _result = ExecuteActionTile(tile);
        }

        private InstructionTileResult ExecuteActionTile(IInstructionTile tile)
        {
            if(tile == null)
            {
                _result = InstructionTileResult.Success;
                return InstructionTileResult.Success;
            }
            else
            {
                var tileResult = tile.Do();
                //Debug.Log($"{nameof(ExecuteActionTile)} {tile} result {tileResult}");
                if (tileResult == InstructionTileResult.Success)
                    tileResult = ExecuteActionTile(tile.NextExecutionTile);
                return tileResult;
            }
        }

        public void AddTile(IInstructionTile tile)
        {
            var instance = (IInstructionTile)Activator.CreateInstance(tile.TileType);
            instance.Id = tile.Id;
            instance.Name = tile.Name;
            instance.Category = tile.Category;

            CopyBrainProperties(tile, instance);

            if(instance is IConditionInstructionTile)
            {
                var idx = InstructionTiles.FindLastIndex(x => x is IConditionInstructionTile);
                if(idx > -1)
                {
                    InstructionTiles.Insert(idx+1, instance);
                    return;
                }
            }

            InstructionTiles.Add(instance);
        }

        public void DeleteTile(int i)
        {
            if (i >= 0 && i < InstructionTiles.Count)
                InstructionTiles.RemoveAt(i);
        }

        public void MoveTileRight(int sourceIndex)
        {
            if (sourceIndex < InstructionTiles.Count - 1)
            {
                var targetTile = InstructionTiles[sourceIndex + 1];
                var sourceTile = InstructionTiles[sourceIndex];
                InstructionTiles.RemoveAt(sourceIndex);
                InstructionTiles.Insert(InstructionTiles.IndexOf(targetTile)+1, sourceTile);
            }
        }

        public void MoveTileLeft(int sourceIndex)
        {
            if (sourceIndex > 0)
            {
                var targetTile = InstructionTiles[sourceIndex - 1];
                var sourceTile = InstructionTiles[sourceIndex];
                InstructionTiles.RemoveAt(sourceIndex);
                InstructionTiles.Insert(InstructionTiles.IndexOf(targetTile), sourceTile);
            }
        }

        public void ReplaceTile(int i, IInstructionTile tile)
        {
            var instance = (IInstructionTile)Activator.CreateInstance(tile.TileType);
            instance.Id = tile.Id;
            instance.Name = tile.Name;
            instance.Category = tile.Category;

            CopyBrainProperties(InstructionTiles[i], instance);
            InstructionTiles[i] = instance;
        }

        private void CopyBrainProperties(IInstructionTile source, IInstructionTile target)
        {
            var properties = target.GetType().GetProperties();
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var attributes = property.GetCustomAttributes(typeof(BrainProperty), true);
                if (attributes.Length == 0)
                {
                    attributes = property.GetCustomAttributes(typeof(BrainPropertyValueName), true);
                    if (attributes.Length == 0)
                        continue;
                }
                var sourceProperty = source.GetType().GetProperty(property.Name);
                if(sourceProperty != null)
                    property.SetValue(target, sourceProperty.GetValue(source));
            }
        }
    }
}