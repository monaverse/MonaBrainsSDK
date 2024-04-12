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
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.Tiles.Actions.Audio
{
    [Serializable]
    public class PlayAudioInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IPauseableInstructionTile, IActivateInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "PlayAudio";
        public const string NAME = "Play Audio";
        public const string CATEGORY = "Audio";
        public override Type TileType => typeof(PlayAudioInstructionTile);

        public PlayAudioInstructionTile() { }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaAudioAssetItem))] public string AudioClip { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private float _volume = 1f;
        [BrainProperty(true)] public float Volume { get => _volume; set => _volume = value; }

        [SerializeField] private bool _wait = true;
        [BrainProperty(false)] public bool Wait { get => _wait; set => _wait = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyAnimationTriggeredEvent> OnAnimationTriggered;

        private IMonaBrain _brain;
        private IMonaAudioAssetItem _clip;

        private bool _isPlaying;
        private bool _active;
        private bool _canInterrupt = true;

        private AudioSource _audioSource;

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _canInterrupt = instruction.HasConditional();

            SetupAudioSource();
            SetupClip();

            UpdateActive();
        }

        private void SetupAudioSource()
        {
            _audioSource = _brain.Body.ActiveTransform.GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = _brain.Body.ActiveTransform.AddComponent<AudioSource>();
            _audioSource.dopplerLevel = 0;
            _audioSource.pitch = 1;
        }

        private void SetupClip()
        {
            _clip = (IMonaAudioAssetItem)_brain.GetMonaAsset(_monaAsset);
            _audioSource.clip = _clip.Value;
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
            var audioSource = _brain.Body.ActiveTransform.GetComponent<AudioSource>();
            GameObject.Destroy(audioSource);
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            if(_audioSource.clip == _clip.Value)
            {
                if (_brain.LoggingEnabled)
                    Debug.Log($"audio pause {_clip}");
                _audioSource.Pause();
            }
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            if (_audioSource.clip == _clip.Value)
            {
                if (_brain.LoggingEnabled)
                    Debug.Log($"audio unpause {_clip}");
                _audioSource.UnPause();
            }
            UpdateActive();
            return _isPlaying;
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
                if (!_audioSource.isPlaying || (_audioSource.clip == _clip.Value && _audioSource.time >= _audioSource.clip.length))
                {
                    Debug.Log($"audio finished {_clip.Value}");
                    _isPlaying = false;
                    if(_wait)
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
                try
                {
                    if (_canInterrupt || !_audioSource.isPlaying)
                    {
                        if (_brain.LoggingEnabled)
                            Debug.Log($"{nameof(PlayAudioInstructionTile)} play audio {_clip.Value}");
                        SetupClip();
                        _audioSource.volume = _volume;
                        _audioSource.Play();
                        _isPlaying = true;
                        AddFixedTickDelegate();
                        if (_wait)
                            return Complete(InstructionTileResult.Running);
                        else
                            return Complete(InstructionTileResult.Success);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Could not play AUDIO: {e.Message}");
                }
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Running);
        }
    }
}