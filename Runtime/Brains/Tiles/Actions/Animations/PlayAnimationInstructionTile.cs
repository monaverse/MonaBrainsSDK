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
    public class PlayAnimationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "PlayAnimation";
        public const string NAME = "Play Animation";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(PlayAnimationInstructionTile);

        public PlayAnimationInstructionTile() { }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaAnimationAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private float _animationSpeed = 1f;
        [BrainProperty(true)] public float AnimationSpeed { get => _animationSpeed; set => _animationSpeed = value; }

        [SerializeField] private bool _wait = false;
        [BrainProperty(false)] public bool WaitForComplete{ get => _wait;  set => _wait = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyAnimationTriggeredEvent> OnAnimationTriggered;
        private Action<MonaValueChangedEvent> OnMonaValueChanged;

        private IMonaBrain _brain;
        private IMonaAnimationAssetItem _clip;

        private bool _isPlaying;
        private bool _hasPlayed;
        private bool _active;
        private Transform _root;
        private bool _canInterrupt = true;

        private IMonaAnimationController _monaAnimationController;

        private IMonaVariablesBoolValue _triggerValue;
        private bool _currentValue;

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _canInterrupt = instruction.HasConditional();

            SetupAnimation();
            SetupClip();

            RegisterAnimatorCallback();
          
            UpdateActive();
        }

        private void SetupAnimation()
        {
            _root = _brain.Root;
            _monaAnimationController = _root.GetComponent<IMonaAnimationController>();
            _monaAnimationController.SetBrain(_brain);
        }

        private void SetupClip()
        {
            _clip = (IMonaAnimationAssetItem)_brain.GetMonaAsset(_monaAsset);
        }

        private void RegisterAnimatorCallback()
        {
            switch (_brain.PropertyType)
            {

                default: _monaAnimationController.RegisterAnimatorCallback(_clip); break;
            }            
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

            if (_isPlaying)
            {
                AddFixedTickDelegate();
            }

            OnMonaValueChanged = HandleMonaValueChanged;
            EventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }

        public override void Unload()
        {
            var controller = _root.GetComponent<MonaDefaultAnimationController>();
            GameObject.Destroy(controller);
            RemoveFixedTickDelegate();

            EventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return (_active && _isPlaying);
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveFixedTickDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            switch(_brain.PropertyType)
            {

                default: HandleDefaultFixedTick(); break;
            }
        }
        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {

        }

        private void HandleDefaultFixedTick()
        {
            if (_isPlaying)
            {        
                if(_monaAnimationController.HasPlayedAnimation(_clip))
                {
                    _hasPlayed = true;
                }
                if (_monaAnimationController.HasEnded(_clip) && _hasPlayed)
                {
                    //Debug.Log($"animation finished {_clip.Value}");
                    _isPlaying = false;
                    _monaAnimationController.SetLayerWeight(_clip.Layer, 0f);
                    Complete(InstructionTileResult.Success, true);
                }
            }
        }

        public override InstructionTileResult Do()
        {
            switch (_brain.PropertyType)
            {

                default: return DefaultDo(); break;
            }
        }

        private InstructionTileResult DefaultDo()
        {
            if (!_isPlaying)
            {
                //Debug.Log($"{nameof(PlayAnimationInstructionTile)} do {_clip.Value} {_canInterrupt}");
                if (_monaAnimationController.Play(_clip, _canInterrupt, _animationSpeed, isNetworked:true))
                {
                    //if(_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(PlayAnimationInstructionTile)} play animation {_clip.Value}");

                    AddFixedTickDelegate();
                    if (!_canInterrupt || _wait)
                    {
                        _isPlaying = true;
                        _hasPlayed = false;
                        return Complete(InstructionTileResult.Running);
                    }
                    else
                    {
                        _isPlaying = false;
                        return Complete(InstructionTileResult.Success);
                    }
                }
                if (_wait)
                    return Complete(InstructionTileResult.Failure);
                else
                    return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Running);
        }
    }
}