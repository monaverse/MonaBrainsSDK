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
    public class SetAnimatorFloatInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "SetAnimatorFloat";
        public const string NAME = "Set Animator Float";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(SetAnimatorFloatInstructionTile);

        public SetAnimatorFloatInstructionTile() { }

        public bool IsAnimationTile => true;

        [SerializeField] private string _floatName = null;
        [BrainProperty(true)] public string FloatName { get => _floatName; set => _floatName = value; }

        [SerializeField] private float _value = 0;
        [SerializeField] private string _valueName;
        [BrainProperty(true)] public float Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueName { get => _valueName; set => _valueName = value; }


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
            if(_brain.Body.Animator != null && evt.Value is IMonaVariablesFloatValue && evt.Name == _floatName)
            {
                _brain.Body.Animator.SetFloat(_floatName, ((IMonaVariablesFloatValue)evt.Value).Value);
            }
            Debug.Log($"{nameof(HandleMonaValueChanged)}", _brain.Body.Transform.gameObject);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _brain.Body.Animator == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_valueName))
                _value = _brain.Variables.GetFloat(_valueName);

            _brain.Body.Animator.SetFloat(_floatName, _value);

            return Complete(InstructionTileResult.Success);
        }
    }
}