using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Animation
{
    public interface IMonaAnimationController
    {
        Animator Animator { get; }
        bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed);
        bool HasEnded(IMonaAnimationAssetItem clipItem);
        bool HasPlayedAnimation(IMonaAnimationAssetItem clipItem);
        void SetBrain(IMonaBrain brain);
        void RegisterAnimatorCallback(IMonaAnimationAssetItem clipItem);
        void SetAnimator(Animator animator);

        void SetLayerWeight(int layer, float layerWeight);
        void SetTPose(bool value);
        void SetWalk(float speed);
        void SetMotionSpeed(float speed);
        void Jump();
        void Landed();

    }

    public class MonaDefaultAnimationController : MonoBehaviour, IMonaAnimationController
    {
        private Animator _animator;
        public Animator Animator => _animator;

        private AnimatorOverrideController _controller;

        private const string START_STATE = "__Start";
        private const string END_STATE = "__End";
        private const string CLIP_STATE = "__Clip";
        private const string TRIGGER = "__TriggerAnimation";
        private const string ANIMATION_SPEED = "__AnimationSpeed";

        private Action<MonaValueChangedEvent> OnMonaValueChanged;

        private Dictionary<string, IMonaAnimationAssetItem> _animationAssets = new Dictionary<string, IMonaAnimationAssetItem>();
        private IMonaBrain _brain;

        public void Awake()
        {
        }

        public void SetBrain(IMonaBrain brain)
        {
            if (_brain == null)
            {
                SetupAnimationController();
                OnMonaValueChanged = HandleMonaValueChanged;
                EventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, brain.Body), OnMonaValueChanged);
                _brain = brain;
                _brain.Body.SetAnimator(_animator);
                _brain.Variables.Set(TRIGGER, "");
            }
        }

        private void OnDestroy()
        {
            if (_brain != null)
                EventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }

        private void SetupAnimationController(Animator animator = null)
        {
            if (animator == null)
            {
                _animator = gameObject.GetComponent<Animator>();
                if (_animator == null)
                    _animator = gameObject.AddComponent<Animator>();
            }
            else
            {
                if (_animator != null)
                {
                    Destroy(_animator);
                }
                _animator = animator;
                _brain.Body.SetAnimator(_animator);
            }

            if (_animator.runtimeAnimatorController == null)
            {
                var controller = (RuntimeAnimatorController)GameObject.Instantiate(Resources.Load("MonaDefaultAnimationController", typeof(RuntimeAnimatorController)));
                if (controller == null)
                {
                    Debug.LogError($"{nameof(MonaDefaultAnimationController)} Cannot find Resource MonaDefaultAnimationController, please make sure to import the MonaBodySDK Starter Sample");
                    return;
                }
                var overrideController = new AnimatorOverrideController(controller);
                _animator.runtimeAnimatorController = overrideController;
            }
            _controller = (AnimatorOverrideController)_animator.runtimeAnimatorController;
        }

        public void SetAnimator(Animator animator)
        {
            SetupAnimationController(animator);
        }

        public void SetWalk(float speed)
        {

        }

        public void SetMotionSpeed(float speed)
        {

        }

        public void Jump()
        {

        }

        public void Landed()
        {

        }

        public void SetTPose(bool value)
        {

        }

        public void SetLayerWeight(int layer, float layerWeight)
        {
            _animator.SetLayerWeight(layer, layerWeight);
        }

        public bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed = 1f)
        {
            if (_controller == null) return false;

            if (canInterrupt)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);
                if (_controller[CLIP_STATE].name == clipItem.Value.name && (!HasEnded(clipItem) || current.IsName(START_STATE))) return false;
                //Debug.Log($"Trigger {clipItem.Value.name}");
                _controller[CLIP_STATE] = clipItem.Value;
                _animator.SetTrigger(TRIGGER);
                _animator.SetFloat(ANIMATION_SPEED, speed);
                _brain.Variables.Set(TRIGGER, clipItem.Value.name);
                return true;
            }
            else
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);
                if (HasEnded(clipItem) || current.IsName(START_STATE))
                {
                    //Debug.Log($"transition time {transition.normalizedTime}");
                    //Debug.Log($"play {clipItem.Value.name}");
                    _brain.Variables.Set(TRIGGER, clipItem.Value.name);
                    _animator.SetTrigger(TRIGGER);
                    _animator.SetFloat(ANIMATION_SPEED, speed);
                    _controller[CLIP_STATE] = clipItem.Value;
                    return true;
                }
            }
            return false;
        }

        public bool HasPlayedAnimation(IMonaAnimationAssetItem clipItem)
        {
            var current = _animator.GetCurrentAnimatorStateInfo(clipItem.Layer);
            return current.IsName(CLIP_STATE);
        }

        public bool HasEnded(IMonaAnimationAssetItem clipItem)
        {
            var transition = _animator.GetAnimatorTransitionInfo(clipItem.Layer);
            var current = _animator.GetCurrentAnimatorStateInfo(clipItem.Layer);
            return (current.IsName(END_STATE)) && transition.normalizedTime == 0;
        }

        public void RegisterAnimatorCallback(IMonaAnimationAssetItem clipItem)
        {
            if (!_animationAssets.ContainsKey(clipItem.Value.name))
                _animationAssets.Add(clipItem.Value.name, clipItem);
        }

        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {
            if(evt.Name == TRIGGER)
            {
                var animation = ((IMonaVariablesStringValue)evt.Value).Value;
                if (_animationAssets.ContainsKey(animation) && (_controller[CLIP_STATE] == null || _controller[CLIP_STATE].name != animation))
                    Play(_animationAssets[animation], true);
            }
        }

    }
}