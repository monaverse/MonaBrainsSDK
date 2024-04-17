﻿using Mona.SDK.Brains.Core.Enums;
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

namespace Mona.SDK.Brains.Tiles.Actions.Animations
{
    [Serializable]
    public class SetAnimatorIntegerInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "SetAnimatorInteger";
        public const string NAME = "Set Animator Integer";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(SetAnimatorIntegerInstructionTile);

        public SetAnimatorIntegerInstructionTile() { }

        public bool IsAnimationTile => true;

        [SerializeField] private string _integerName = null;
        [BrainProperty(true)] public string IntegerName { get => _integerName; set => _integerName = value; }

        [SerializeField] private int _value = 0;
        [SerializeField] private string _valueName;
        [BrainProperty(true)] public int Value { get => _value; set => _value = value; }
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

        public override void Unload()
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

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveRemoteAnimationDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        private void AddRemoteAnimationDelegate()
        {
            OnMonaValueChanged = HandleMonaValueChanged;
            EventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);

        }

        private void RemoveRemoteAnimationDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }

        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {
            if(_brain.Body.Animator != null && evt.Value is IMonaVariablesFloatValue && evt.Name == _integerName)
            {
                _brain.Body.Animator.SetInteger(_integerName, (int)((IMonaVariablesFloatValue)evt.Value).Value);
            }
            Debug.Log($"{nameof(HandleMonaValueChanged)}", _brain.Body.Transform.gameObject);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _brain.Body.Animator == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_valueName))
                _value = (int)_brain.Variables.GetFloat(_valueName);

            _brain.Body.Animator.SetInteger(_integerName, _value);

            return Complete(InstructionTileResult.Success);
        }
    }
}