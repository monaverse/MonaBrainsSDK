using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Animations
{
    [Serializable]
    public class SetAnimatorBoolInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "SetAnimatorBool";
        public const string NAME = "Set Animator Bool";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(SetAnimatorBoolInstructionTile);

        public SetAnimatorBoolInstructionTile() { }

        public bool IsAnimationTile => true;

        [SerializeField] private string _boolName = null;
        [BrainProperty(true)] public string BoolName { get => _boolName; set => _boolName = value; }

        [SerializeField] private bool _boolValue = false;
        [BrainProperty(true)] public bool BoolValue { get => _boolValue; set => _boolValue = value; }

        private Action<MonaValueChangedEvent> OnMonaValueChanged;

        private IMonaBrain _brain;

        private bool _active;
        private Transform _root;
        
        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            
            UpdateActive();
        }

        private void SetupAnimation()
        {
            _root = _brain.Root;
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                if (_brain != null)
                    UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active) return;
            AddRemoteAnimationDelegate();
        }

        public override void Unload(bool destroy = false)
        {
            RemoveRemoteAnimationDelegate();
        }

        public void Pause()
        {
            RemoveRemoteAnimationDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return (_active);
        }

        public override void SetThenCallback(InstructionTileCallback thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback = thenCallback;
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback;
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveRemoteAnimationDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddRemoteAnimationDelegate()
        {
            OnMonaValueChanged = HandleMonaValueChanged;
            MonaEventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);

        }

        private void RemoveRemoteAnimationDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }

        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {
            if(_brain.Body.Animator != null && evt.Value is IMonaVariablesBoolValue && evt.Name == _boolName)
            {
                _brain.Body.Animator.SetBool(_boolName, ((IMonaVariablesBoolValue)evt.Value).Value);
            }
            //Debug.Log($"{nameof(HandleMonaValueChanged)}", _brain.Body.Transform.gameObject);
        }

        public override InstructionTileResult Do()
        {
            _brain.Variables.Set(_boolName, _boolValue);
            return Complete(InstructionTileResult.Success);
        }
    }
}