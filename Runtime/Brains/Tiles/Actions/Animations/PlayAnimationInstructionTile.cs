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
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEditor.Animations;

namespace Mona.SDK.Brains.Tiles.Actions.Animations
{
    [Serializable]
    public class PlayAnimationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "PlayAnimation";
        public const string NAME = "Play Animation";
        public const string CATEGORY = "Animations";
        public override Type TileType => typeof(PlayAnimationInstructionTile);

        public PlayAnimationInstructionTile() { }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaAnimationAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private IMonaBrain _brain;
        private Animator _animator;
        private IMonaAnimationAssetItem _clip;
        private bool _isPlaying;
        private bool _active;
        private AnimatorOverrideController _controller;

        private int _hashEndPoint1 = Animator.StringToHash("Start");
        private int _hashEndPoint2 = Animator.StringToHash("Complete");
        private int _targetEndPoint;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;

            _animator = _brain.Body.ActiveTransform.gameObject.GetComponent<Animator>();
            if (_animator == null)
                _animator = _brain.Body.ActiveTransform.gameObject.AddComponent<Animator>();

            _clip = (IMonaAnimationAssetItem)_brain.GetMonaAsset(_monaAsset);

            _controller = (AnimatorOverrideController)GameObject.Instantiate(Resources.Load("MonaBasicOverrideController"));
            _animator.runtimeAnimatorController = _controller;
            
            UpdateActive();
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
        }


        public override void Unload()
        {
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public void Resume()
        {
            UpdateActive();
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
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            if (_isPlaying)
            {
                var transition = _animator.GetAnimatorTransitionInfo(0);
                var currentState = _animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"{nameof(PlayAnimationInstructionTile)} {transition.normalizedTime}");
                if (currentState.fullPathHash == _targetEndPoint)
                {
                    var clip = _controller["Transitioning"];
                    Debug.Log($"{nameof(PlayAnimationInstructionTile)} played animation {clip}");
                    _isPlaying = false;
                    Complete(InstructionTileResult.Success);
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                _controller["Complete"] = _controller["Transitioning"];
                _controller["Transitioning"] = _clip.Value;
                _animator.SetTrigger("Go");
                _targetEndPoint = _animator.GetNextAnimatorStateInfo(0).fullPathHash;
            }
            return Complete(InstructionTileResult.Running);
        }
    }
}