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
    public class SetAnimatorTriggerInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "SetAnimatorTrigger";
        public const string NAME = "Set Animator Trigger";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(SetAnimatorTriggerInstructionTile);

        public SetAnimatorTriggerInstructionTile() { }

        public bool IsAnimationTile => true;

        [SerializeField] private string _triggerName = null;
        [BrainProperty(true)] public string TriggerName { get => _triggerName; set => _triggerName = value; }

        private Action<MonaBodyAnimationTriggeredEvent> OnRemoteAnimation;

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
            OnRemoteAnimation = HandleRemoteAnimationTriggered;
            MonaEventBus.Register<MonaBodyAnimationTriggeredEvent>(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnRemoteAnimation);
        }

        private void RemoveRemoteAnimationDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnRemoteAnimation);
        }

        private void HandleRemoteAnimationTriggered(MonaBodyAnimationTriggeredEvent evt)
        {
            var animation = evt.ClipName;
            Debug.Log($"{nameof(HandleRemoteAnimationTriggered)} {animation}", _brain.Body.Transform.gameObject);
            if(_brain.Body.Animator != null)
                _brain.Body.Animator.SetTrigger(evt.ClipName);
        }

        public override InstructionTileResult Do()
        {
            //Debug.Log($"{nameof(SetAnimatorTriggerInstructionTile)} {_triggerName}", _brain.Body.Transform.gameObject);
            if (_brain.Body.Animator != null)
                _brain.Body.Animator.SetTrigger(_triggerName);
            _brain.Body.TriggerRemoteAnimation(_triggerName);
            return Complete(InstructionTileResult.Success);
        }
    }
}