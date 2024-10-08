﻿using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using Unity.VisualScripting;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Utils;
using Unity.Profiling;
using Mona.SDK.Brains.Core.Utils;

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

        [SerializeField] private AudioClassificationSubType _audioType = AudioClassificationSubType.SoundEffect;
        [BrainPropertyEnum(true)] public AudioClassificationSubType AudioType { get => _audioType; set => _audioType = value; }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaAudioAssetItem))] public string AudioClip { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private float _volume = 1f;
        [SerializeField] private string _volumeName;
        [BrainProperty(true)] public float Volume { get => _volume; set => _volume = value; }
        [BrainPropertyValueName("Volume", typeof(IMonaVariablesFloatValue))] public string VolumeName { get => _volumeName; set => _volumeName = value; }

        [SerializeField] private float _pitch = 1f;
        [SerializeField] private string _pitchName;
        [BrainProperty(false)] public float Pitch { get => _pitch; set => _pitch = value; }
        [BrainPropertyValueName("Pitch", typeof(IMonaVariablesFloatValue))] public string PitchName { get => _pitchName; set => _pitchName = value; }

        [SerializeField] private bool _allowInterruption = true;
        [SerializeField] private string _allowInterruptionName;
        [BrainProperty(false)] public bool AllowInterruption { get => _allowInterruption; set => _allowInterruption = value; }
        [BrainPropertyValueName("AllowInterruption", typeof(IMonaVariablesBoolValue))] public string AllowInterruptionName { get => _allowInterruptionName; set => _allowInterruptionName = value; }

        [SerializeField] private bool _loopAudio = false;
        [SerializeField] private string _loopAudioName;
        [BrainProperty(false)] public bool LoopAudio { get => _loopAudio; set => _loopAudio = value; }
        [BrainPropertyValueName("LoopAudio", typeof(IMonaVariablesBoolValue))] public string LoopAudioName { get => _loopAudioName; set => _loopAudioName = value; }

        [SerializeField] private bool _pauseable = true;
        [SerializeField] private string _pauseableName;
        [BrainProperty(false)] public bool Pauseable { get => _pauseable; set => _pauseable = value; }
        [BrainPropertyValueName("Pausable", typeof(IMonaVariablesBoolValue))] public string PauseableName { get => _pauseableName; set => _pauseableName = value; }

        [SerializeField] private bool _wait = false;
        [SerializeField] private string _waitName;
        [BrainProperty(false)] public bool Wait { get => _wait; set => _wait = value; }
        [BrainPropertyValueName("Wait", typeof(IMonaVariablesBoolValue))] public string WaitName { get => _waitName; set => _waitName = value; }

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyAnimationTriggeredEvent> OnAnimationTriggered;

        private IMonaBrain _brain;
        private IMonaAudioAssetItem _clip;
        private MonaBrainAudio _brainAudio;
        private AudioObjectInfo _audioObject;

        private bool _isPlaying;
        private bool _active;
        private bool _canInterrupt = true;

        private AudioSource _audioSource;
        private BrainsAudioLoader _audioLoader;

        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(PlayAudioInstructionTile)}.{nameof(Do)}");
        static readonly ProfilerMarker _profilerPreload = new ProfilerMarker($"MonaBrains.{nameof(PlayAudioInstructionTile)}.{nameof(Preload)}");

        private AudioClassificationType TrueAudioClassType => (AudioClassificationType)(int)_audioType;

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _profilerPreload.Begin();

            _brain = brain;
            _canInterrupt = instruction.HasConditional();

            SetupAudioSource();
            SetupAudioLoader();
            SetupClip();
            SetupAudioObject();
            UpdateActive();

            _profilerPreload.End();
        }

        private void SetupAudioSource()
        {
            _audioSource = _brain.Body.ActiveTransform.GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = _brain.Body.ActiveTransform.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.dopplerLevel = 0;
        }

        private void SetupAudioLoader()
        {
            _audioLoader = _brain.Body.ActiveTransform.GetComponent<BrainsAudioLoader>();
            if (_audioLoader == null)
                _audioLoader = _brain.Body.ActiveTransform.AddComponent<BrainsAudioLoader>();
        }

        private void SetupClip()
        {
            _clip = (IMonaAudioAssetItem)_brain.GetMonaAsset(_monaAsset);

            if (!string.IsNullOrEmpty(_clip.Url))
                LoadClip(_clip.Url);
            else
                _audioSource.clip = _clip.Value;
        }

        private void SetupAudioObject()
        {
            _brainAudio = MonaBrainAudio.Instance;

            if (_brainAudio == null)
                return;

            _audioObject = _brainAudio.GetAudioObject(_brain.Body.Transform.gameObject);

            if (_audioObject != null)
            {
                _audioObject.Type = TrueAudioClassType;
                return;
            }

            _audioObject = _brainAudio.CreateNewAudioObject(_audioSource, TrueAudioClassType, 1f);
        }

        private void LoadClip(string url)
        {
            _audioLoader.GetAudioData(url, (clip) =>
            {
                _audioSource.clip = clip;
            });
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


        public override void Unload(bool destroy = false)
        {
            _isPlaying = false;
            _active = false;

            //if (destroy)
            //{
            //    var audioSource = _brain.Body.ActiveTransform.GetComponent<AudioSource>();
            //    GameObject.Destroy(audioSource, 0.1f);
            //}

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

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }
        
        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

        }

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
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
                    //Debug.Log($"audio finished {_clip.Value}");
                    _isPlaying = false;
                    if(_wait)
                        Complete(InstructionTileResult.Success, true);
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_volumeName))
                _volume = _brain.Variables.GetFloat(_volumeName);

            if (!string.IsNullOrEmpty(_pitchName))
                _pitch = _brain.Variables.GetFloat(_pitchName);

            if (!string.IsNullOrEmpty(_loopAudioName))
                _loopAudio = _brain.Variables.GetBool(_loopAudioName);

            if (!string.IsNullOrEmpty(_pauseableName))
                _pauseable = _brain.Variables.GetBool(_pauseableName);

            if (!string.IsNullOrEmpty(_waitName))
                _wait = _brain.Variables.GetBool(_waitName);

            if (!string.IsNullOrEmpty(_allowInterruptionName))
                _allowInterruption = _brain.Variables.GetBool(_allowInterruptionName);
            
            if (!_isPlaying || _allowInterruption)
            {
                try
                {
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(PlayAudioInstructionTile)} play audio {_clip.Value}");

                    try
                    {
                        SetupClip();

                        _audioObject.UpdateVolume(_volume);
                        _audioObject.Source.pitch = _pitch;
                        _audioObject.Source.loop = _loopAudio;
                        _audioObject.Pausable = _pauseable;
                        _audioObject.Source.Play();

                        _isPlaying = true;

                        AddFixedTickDelegate();
                        if (_wait)
                            return Complete(InstructionTileResult.Running);
                        else
                            return Complete(InstructionTileResult.Success);
                    }
                    catch
                    {
                        return Complete(InstructionTileResult.Success);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Could not play AUDIO: {e.Message}");
                }
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}