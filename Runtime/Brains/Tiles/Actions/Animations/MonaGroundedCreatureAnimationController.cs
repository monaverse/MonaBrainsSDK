using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Animation
{
    public class MonaGroundedCreatureAnimationController : MonoBehaviour, IMonaAnimationController
    {
        private Animator _animator;
        public Animator Animator => _animator;

        private AnimatorOverrideController _controller;

        private const string START_STATE = "__Start";
        private const string END_STATE = "Idle Walk Run Blend";
        private const string CLIP_STATE = "__Clip";

        private const string SPEED = "Speed";
        private const string JUMP = "Jump";
        private const string GROUNDED = "Grounded";
        private const string FREE_FALL = "Freefall";
        private const string MOTION_SPEED = "MotionSpeed";
        private const string EMOTE = "Emote";

        private const string TPOSE = "TPose";
        private const string TPOSE_TRIGGER = "__TriggerTPose";

        private Action<MonaBodyAnimationTriggeredEvent> OnRemoteAnimation;

        private Dictionary<string, IMonaAnimationAssetItem> _animationAssets = new Dictionary<string, IMonaAnimationAssetItem>();
        private IMonaBrain _brain;

        public void Awake()
        {
        }

        public void SetBrain(IMonaBrain brain)
        {
            Debug.Log($"{nameof(MonaGroundedCreatureAnimationController)}.{nameof(SetBrain)}");
            if (_brain == null)
            {
                SetupAnimationController();
                _brain = brain;

                OnRemoteAnimation = HandleRemoteAnimationTriggered;
                EventBus.Register<MonaBodyAnimationTriggeredEvent>(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnRemoteAnimation);

                _brain.Body.SetAnimator(_animator);
                _brain.Variables.Set(MonaBrainConstants.TRIGGER, "");
                _brain.Variables.Set(MonaBrainConstants.ANIMATION_SPEED, 1f);
                Debug.Log($"I WAS CALLLED {MonaBrainConstants.TRIGGER} = {_brain.Variables.GetString(MonaBrainConstants.TRIGGER)}");
            }
        }

        private void OnDestroy()
        {
            Debug.Log($"{nameof(MonaGroundedCreatureAnimationController)}.{nameof(OnDestroy)}");
            if (_brain != null)
                EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnRemoteAnimation);
            _brain = null;
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
                    Destroy(_animator);
                _animator = animator;
                _brain.Body.SetAnimator(_animator);
            }

            if (_animator.runtimeAnimatorController == null)
            {
                var controller = (RuntimeAnimatorController)GameObject.Instantiate(Resources.Load("MonaPlayer/MonaGroundedHumanoidAnimationController", typeof(RuntimeAnimatorController)));
                if (controller == null)
                {
                    Debug.LogError($"{nameof(MonaGroundedCreatureAnimationController)} Cannot find Resource MonaGroundedHumanoidAnimationController, please make sure to import the MonaBodySDK Starter Sample");
                    return;
                }
                var overrideController = new AnimatorOverrideController(controller);
                _animator.runtimeAnimatorController = overrideController;
                _animator.Rebind();
            }
            _controller = (AnimatorOverrideController)_animator.runtimeAnimatorController;
        }

        public void SetAnimator(Animator animator)
        {
            SetupAnimationController(animator);
        }

        private void Update()
        {
            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            _speed = Mathf.Lerp(_speed, _toSpeed, Time.deltaTime*10f);
            _animator.SetFloat(SPEED, _speed);
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
            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetFloat(MOTION_SPEED, speed);
        }

        public void Jump()
        {
            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(GROUNDED, false);
            _animator.SetBool(JUMP, true);
        }

        public void Landed()
        {
            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetBool(JUMP, false);
            _animator.SetBool(GROUNDED, true);
        }

        public void SetLayerWeight(int layer, float layerWeight)
        {
            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            _animator.SetLayerWeight(layer, layerWeight);
        }

        public void SetTPose(bool value)
        {
            var pose = (IMonaAnimationAssetItem)_brain.GetMonaAsset(MonaAnimationAssets.TPOSE);
            if (pose != null)
                _controller[TPOSE] = pose.Value;
            _animator.SetBool(TPOSE_TRIGGER, value);
        }

        public bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed = 1f, bool isNetworked = true)
        {
            if (_controller == null) return false;
            
            if (canInterrupt)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(clipItem.Layer);
                if (_controller[CLIP_STATE].name == clipItem.Value.name && (!HasEnded(clipItem) || current.IsName(START_STATE))) return false;
                //Debug.Log($"Trigger {clipItem.Value.name}");
                _controller[CLIP_STATE] = clipItem.Value;
                _animator.SetLayerWeight(clipItem.Layer, clipItem.LayerWeight);
                _animator.SetFloat(MonaBrainConstants.ANIMATION_SPEED, speed);

                if (clipItem.Layer == 0) _animator.SetTrigger(MonaBrainConstants.TRIGGER);
                else if (clipItem.Layer == 1) _animator.SetTrigger(MonaBrainConstants.TRIGGER_1);

                if(isNetworked)
                    _brain.Body.TriggerRemoteAnimation(clipItem.Value.name);
                return true;
            }
            else
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);
                if (HasEnded(clipItem) || current.IsName(START_STATE))
                {
                    //Debug.Log($"transition time {transition.normalizedTime}");
                    //Debug.Log($"play {clipItem.Value.name}");
                    _controller[CLIP_STATE] = clipItem.Value;
                    _animator.SetFloat(MonaBrainConstants.ANIMATION_SPEED, speed);

                    if (clipItem.Layer == 0) _animator.SetTrigger(MonaBrainConstants.TRIGGER);
                    else if (clipItem.Layer == 1) _animator.SetTrigger(MonaBrainConstants.TRIGGER_1);

                    if(isNetworked)
                        _brain.Body.TriggerRemoteAnimation(clipItem.Value.name);
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

        private void HandleRemoteAnimationTriggered(MonaBodyAnimationTriggeredEvent evt)
        {
            var animation = evt.ClipName;
            Debug.Log($"{nameof(HandleRemoteAnimationTriggered)} {animation}", _brain.Body.Transform.gameObject);
            if (_animationAssets.ContainsKey(animation))
                Play(_animationAssets[animation], true, isNetworked:false);
        }

    }
}