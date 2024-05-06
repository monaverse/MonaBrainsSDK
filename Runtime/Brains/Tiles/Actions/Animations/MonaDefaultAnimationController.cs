using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Core.Animation
{
    public interface IMonaAnimationController
    {
        Animator Animator { get; }
        MonaBrainPropertyType PropertyType { get; }
        AnimatorOverrideController Controller { get; set; }
        AnimatorOverrideController OldController { get; set; }
        bool ReuseController { get; set; }

        bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed, bool isNetworked);
        bool HasEnded(IMonaAnimationAssetItem clipItem);
        bool HasPlayedAnimation(IMonaAnimationAssetItem clipItem);
        void SetBrain(IMonaBrain brain, Animator animator = null);
        void RegisterAnimatorCallback(IMonaAnimationAssetItem clipItem);
        void SetAnimator(Animator animator);

        void SetLayerWeight(int layer, float layerWeight);
        void SetTPose(bool value);
        void SetWalk(float speed);
        void SetMotionSpeed(float speed);
        void Jump();
        void Landed();
        void Idle();
        void IdleOff();

    }

    public class MonaDefaultAnimationController : MonoBehaviour, IMonaAnimationController
    {
        private Animator _animator;
        public Animator Animator => _animator;

        private AnimatorOverrideController _controller;
        public AnimatorOverrideController Controller
        {
            get => _controller;
            set => _controller = value;
        }

        private AnimatorOverrideController _oldController;
        public AnimatorOverrideController OldController
        {
            get => _oldController;
            set => _oldController = value;
        }

        private bool _reuseController;
        public bool ReuseController
        {
            get => _reuseController;
            set => _reuseController = value;
        }

        private const string START_STATE = "__Start";
        private const string WALK_STATE = "Walk";
        private const string END_STATE = "__End";
        private const string CLIP_STATE = "__Clip";

        private const string IDLE = "Idle";
        private const string SPEED = "Speed";
        private const string JUMP = "Jump";
        private const string GROUNDED = "Grounded";
        private const string FREE_FALL = "Freefall";
        private const string MOTION_SPEED = "MotionSpeed";
        private const string EMOTE = "Emote";

        private Action<MonaValueChangedEvent> OnMonaValueChanged;

        private Dictionary<string, IMonaAnimationAssetItem> _animationAssets = new Dictionary<string, IMonaAnimationAssetItem>();
        private IMonaBrain _brain;
        private bool _override = true;

        public void Awake()
        {
        }

        public MonaBrainPropertyType PropertyType => _brain.PropertyType;

        public void SetBrain(IMonaBrain brain, Animator animator = null)
        {
            if (_brain == null)
            {
                _brain = brain;
            }

            SetupAnimationController(animator);
            OnMonaValueChanged = HandleMonaValueChanged;
            MonaEventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, brain.Body), OnMonaValueChanged);
            _brain.Variables.Set(MonaBrainConstants.TRIGGER, "");
            _brain.Variables.Set(MonaBrainConstants.ANIMATION_SPEED, 1f);
            _brain.Body.SetAnimator(_animator);
        }

        public void SetOverride(bool value)
        {
            _override = value;
        }

        private void OnDestroy()
        {
            if (_brain != null)
                MonaEventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain.Body), OnMonaValueChanged);
        }


        private void SetupAnimationController(Animator animator = null)
        {
            if (animator == null)
            {
                var bodyAnimator = _brain.Body.Transform.GetComponent<Animator>();
                if (bodyAnimator != null)
                    _animator = bodyAnimator;
                else
                {
                    var childAnimator = gameObject.GetComponentInChildren<Animator>();
                    if (childAnimator != null && (childAnimator.transform.parent == transform || childAnimator.transform == transform))
                        _animator = childAnimator;
                }
            }
            else
            {
                if (_animator != null && _animator != animator)
                {
                    if(_animator.gameObject != null)
                    {
                        if (_animator.gameObject != gameObject)
                            DestroyImmediate(_animator.gameObject);
                        else
                        {
                            DestroyImmediate(_animator);
                        }
                    }
                }
                _animator = animator;

                if (_oldController != null && _reuseController)
                {
                    RuntimeAnimatorController oldController = null;
                    if (_oldController is AnimatorOverrideController)
                        oldController = ((AnimatorOverrideController)_oldController).runtimeAnimatorController;
                    else
                        oldController = _oldController.runtimeAnimatorController;

                    if (oldController != null)
                        _animator.runtimeAnimatorController = oldController;
                }

                _brain.Body.SetAnimator(_animator);
            }

            if (_animator != null && _animator.runtimeAnimatorController != null && _reuseController)
            {
                var overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                _animator.runtimeAnimatorController = overrideController;
            }

            if (_animator != null && _animator.runtimeAnimatorController is AnimatorOverrideController)
                _controller = (AnimatorOverrideController)_animator.runtimeAnimatorController;
        }

        public void SetAnimator(Animator animator)
        {
            SetupAnimationController(animator);
        }

        private float _speed = 0f;
        private float _toSpeed = 0f;
        public void SetWalk(float speed)
        {
            if (speed > 0f)
            {
                _speed = speed;
                _toSpeed = speed;
            }
            else
            {
                _toSpeed = 0f;
            }
        }

        public void SetMotionSpeed(float speed)
        {
            if (_animator == null || _brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetFloat(MOTION_SPEED, speed);
        }

        public void Jump()
        {
            if (_animator == null || _brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(GROUNDED, false);
            _animator.SetBool(JUMP, true);
        }

        public void Landed()
        {
            if (_animator == null || _brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(JUMP, false);
            _animator.SetBool(GROUNDED, true);
        }

        public void Idle()
        {
            if (_animator == null || _brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(IDLE, true);
        }

        public void IdleOff()
        {
            if (_animator == null || _brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(IDLE, false);
        }

        public void SetTPose(bool value)
        {

        }

        public void SetLayerWeight(int layer, float layerWeight)
        {
            if (_animator == null) return;
            _animator.SetLayerWeight(layer, layerWeight);
        }

        public bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed = 1f, bool force = false)
        {
            if (_animator == null || _controller == null) return false;

            if (canInterrupt)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);
                if (_controller[CLIP_STATE].name == clipItem.Value.name && !current.IsName(WALK_STATE) && (!HasEnded(clipItem) || current.IsName(START_STATE))) return false;
                //Debug.Log($"Trigger {clipItem.Value.name}");
                _controller[CLIP_STATE] = clipItem.Value;
                _animator.SetTrigger(MonaBrainConstants.TRIGGER);
                _animator.SetFloat(MonaBrainConstants.ANIMATION_SPEED, speed);
                _brain.Variables.Set(MonaBrainConstants.TRIGGER, clipItem.Value.name);
                return true;
            }
            else
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);
                if (HasEnded(clipItem) || current.IsName(START_STATE) || current.IsName(WALK_STATE))
                {
                    //Debug.Log($"transition time {transition.normalizedTime}");
                    //Debug.Log($"play {clipItem.Value.name}");
                    _brain.Variables.Set(MonaBrainConstants.TRIGGER, clipItem.Value.name);
                    _animator.SetTrigger(MonaBrainConstants.TRIGGER);
                    _animator.SetFloat(MonaBrainConstants.ANIMATION_SPEED, speed);
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
            if(evt.Name == MonaBrainConstants.TRIGGER)
            {
                var animation = ((IMonaVariablesStringValue)evt.Value).Value;
                if (_animationAssets.ContainsKey(animation) && (_controller[CLIP_STATE] == null || _controller[CLIP_STATE].name != animation))
                    Play(_animationAssets[animation], true);
            }
        }

    }
}