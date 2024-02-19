using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets;
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
    public class MonaGroundedCreatureAnimationController : MonoBehaviour, IMonaAnimationController
    {
        private Animator _animator;
        public Animator Animator => _animator;

        private AnimatorOverrideController _controller;

        private const string START_STATE = "__Start";
        private const string END_STATE = "Idle Walk Run Blend";
        private const string CLIP_STATE = "__Clip";
        private const string TRIGGER = "__TriggerAnimation";
        private const string TRIGGER_1 = "__TriggerAnimation1";
        private const string ANIMATION_SPEED = "__AnimationSpeed";

        private const string SPEED = "Speed";
        private const string JUMP = "Jump";
        private const string GROUNDED = "Grounded";
        private const string FREE_FALL = "Freefall";
        private const string MOTION_SPEED = "MotionSpeed";
        private const string EMOTE = "Emote";

        private const string TPOSE = "TPose";
        private const string TPOSE_TRIGGER = "__TriggerTPose";

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

        private void SetupAnimationController()
        {
            _animator = gameObject.GetComponent<Animator>();
            if (_animator == null)
                _animator = gameObject.AddComponent<Animator>();

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
            }
            _controller = (AnimatorOverrideController)_animator.runtimeAnimatorController;
        }

        public void SetWalk(float speed)
        {
            _animator.SetFloat(SPEED, speed);
        }

        public void SetMotionSpeed(float speed)
        {
            _animator.SetFloat(MOTION_SPEED, speed);
        }

        public void Jump()
        {
            _animator.SetBool(GROUNDED, false);
            _animator.SetBool(JUMP, true);
        }

        public void Landed()
        {
            _animator.SetBool(JUMP, false);
            _animator.SetBool(GROUNDED, true);
        }

        public void SetLayerWeight(int layer, float layerWeight)
        {
            _animator.SetLayerWeight(layer, layerWeight);
        }

        public void SetTPose(bool value)
        {
            var pose = (IMonaAnimationAssetItem)_brain.GetMonaAsset(MonaAnimationAssets.TPOSE);
            if (pose != null)
                _controller[TPOSE] = pose.Value;
            _animator.SetBool(TPOSE_TRIGGER, value);
        }

        public bool Play(IMonaAnimationAssetItem clipItem, bool canInterrupt, float speed = 1f)
        {
            if (_controller == null) return false;

            if (canInterrupt)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(clipItem.Layer);
                if (_controller[CLIP_STATE].name == clipItem.Value.name && (!HasEnded(clipItem) || current.IsName(START_STATE))) return false;
                //Debug.Log($"Trigger {clipItem.Value.name}");
                _controller[CLIP_STATE] = clipItem.Value;
                _animator.SetLayerWeight(clipItem.Layer, clipItem.LayerWeight);

                if (clipItem.Layer == 0) _animator.SetTrigger(TRIGGER);
                else if (clipItem.Layer == 1) _animator.SetTrigger(TRIGGER_1);

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

                    if (clipItem.Layer == 0) _animator.SetTrigger(TRIGGER);
                    else if (clipItem.Layer == 1) _animator.SetTrigger(TRIGGER_1);

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