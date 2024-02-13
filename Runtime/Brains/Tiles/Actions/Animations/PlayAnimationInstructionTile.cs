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
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.Tiles.Actions.Animations
{
    [Serializable]
    public class PlayAnimationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile
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

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyAnimationTriggeredEvent> OnAnimationTriggered;

        private IMonaBrain _brain;
        private IMonaAnimationAssetItem _clip;

        private bool _isPlaying;
        private bool _active;
        private Transform _root;
        private bool _canInterrupt = true;

        private MonaDefaultAnimationController _monaAnimationController;

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _canInterrupt = instruction.HasConditional();

            BuildRoot();

            SetupAnimation();
            SetupClip();
          
            UpdateActive();
        }

        private void BuildRoot()
        {
            if (_brain.Body.Transform.childCount == 1)
            {
                _root = _brain.Body.Transform.GetChild(0);
            }
            else
            {
                _root = (new GameObject("Root")).transform;
                _root.transform.position = _brain.Body.GetPosition();
                _root.transform.rotation = _brain.Body.GetRotation();

                var children = new List<Transform>();
                for (var i = 0; i < _brain.Body.Transform.childCount; i++)
                    children.Add(_brain.Body.Transform.GetChild(i));

                for (var i = 0; i < children.Count; i++)
                    children[i].SetParent(_root, true);

                _root.transform.SetParent(_brain.Body.Transform, true);
            }
        }

        private void SetupAnimation()
        {
            switch (_brain.PropertyType)
            {

                default:
                    _monaAnimationController = _root.GetComponent<MonaDefaultAnimationController>();
                    if (_monaAnimationController == null)
                        _monaAnimationController = _root.AddComponent<MonaDefaultAnimationController>();                        
                break;
            }
        }

        private void SetupClip()
        {
            _clip = (IMonaAnimationAssetItem)_brain.GetMonaAsset(_monaAsset);
            _monaAnimationController.AddClip(_clip, _animationSpeed);
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
            var controller = _root.GetComponent<MonaDefaultAnimationController>();
            GameObject.Destroy(controller);
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

            OnAnimationTriggered = HandleAnimationTriggered;
            EventBus.Register<MonaBodyAnimationTriggeredEvent>(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnAnimationTriggered);
        }

        private void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), OnAnimationTriggered);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            switch(_brain.PropertyType)
            {

                default: HandleDefaultFixedTick(); break;
            }
        }

        private void HandleAnimationTriggered(MonaBodyAnimationTriggeredEvent evt)
        {
            //Debug.Log($"{nameof(PlayAnimationInstructionTile)} interrupted {_clip.Value}");
            _isPlaying = false;
            Complete(InstructionTileResult.Success, true);
        }

        private void HandleDefaultFixedTick()
        {
            if (_isPlaying)
            {               
                if (_monaAnimationController.HasEnded())
                {
                    //Debug.Log($"animation finished {_clip.Value}");
                    _isPlaying = false;
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
            //Debug.Log($"{nameof(PlayAnimationInstructionTile)} do {_clip.Value}");
            if (!_isPlaying)
            {
                if (_monaAnimationController.Play(_clip, _canInterrupt))
                {
                    if(_brain.LoggingEnabled)
                        Debug.Log($"{nameof(PlayAnimationInstructionTile)} play animation {_clip.Value}");
                    EventBus.Trigger<MonaBodyAnimationTriggeredEvent>(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, _brain.Body), new MonaBodyAnimationTriggeredEvent());

                    _isPlaying = true;
                    AddFixedTickDelegate();
                    return Complete(InstructionTileResult.Running);
                }
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Running);
        }
    }
}