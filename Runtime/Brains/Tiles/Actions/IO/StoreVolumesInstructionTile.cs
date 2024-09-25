using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class StoreVolumesInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "StoreVolumes";
        public const string NAME = "Store Volumes";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(StoreVolumesInstructionTile);

        [SerializeField] private StorageTargetType _storageTarget = StorageTargetType.LocalAndCloud;
        [BrainPropertyEnum(true)] public StorageTargetType StorageTarget { get => _storageTarget; set => _storageTarget = value; }

        [SerializeField] private bool _saveNow = true;
        [SerializeField] private string _saveNowName;
        [BrainProperty(false)] public bool SaveNow { get => _saveNow; set => _saveNow = value; }
        [BrainPropertyValueName("SaveNow", typeof(IMonaVariablesBoolValue))] public string SaveNowName { get => _saveNowName; set => _saveNowName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        private const string _keyPrefix = "Audio";
        private const string _keyVolume = "_Volume";
        private const string _keyMuted = "_Muted";

        private const string _keyMaster = "_Master";
        private const string _keySFX = "_SFX";
        private const string _keyVox = "_Vox";
        private const string _keyMusic = "_Music";
        private const string _keyAmbience = "_Ambience";
        private const string _keyUI = "_UserInterface";
        private const string _keyCutscene = "_Cutscene";
        private const string _keyMic = "_Mic";
        private const string _keyChat = "_Chat";
        private const string _keyNotification = "_Notification";
        private const string _keyMisc = "_Misc";

        public StoreVolumesInstructionTile() { }

        private bool _active;
        private bool _isRunning;
        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private MonaBrainAudio _brainAudio;
        private IBrainStorageAsync _localStorage;
        private IBrainStorageAsync _cloudStorage;
        private List<BrainProcess> _localProcesses = new List<BrainProcess>();
        private List<BrainProcess> _cloudProcesses = new List<BrainProcess>();
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool UseLocalStorage => _storageTarget != StorageTargetType.Cloud;
        private bool UseCloudStorage => _storageTarget != StorageTargetType.Local;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            _brainAudio = MonaBrainAudio.Instance;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_isRunning)
                    LostControl();

                return;
            }

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }

        public override void Unload(bool destroy = false)
        {
            SetActive(false);
            _isRunning = false;
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _isRunning;
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

        private void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick();
        }

        private void FixedTick()
        {
            if (UseLocalStorage && (_localStorage == null || ProcessIsActive(_localProcesses)))
                return;

            if (UseCloudStorage && (_cloudStorage == null || ProcessIsActive(_cloudProcesses)))
                return;

            _isRunning = false;
            
            if (!string.IsNullOrEmpty(_storeSuccessOn))
            {
                bool success = UseLocalStorage && ProcessWasSuccessful(_localProcesses) || (UseCloudStorage && ProcessWasSuccessful(_cloudProcesses));
                _brain.Variables.Set(_storeSuccessOn, success);
            }

            _localProcesses.Clear();
            _cloudProcesses.Clear();

            Complete(InstructionTileResult.Success, true);
        }

        public bool ProcessIsActive(List<BrainProcess> processes)
        {
            if (processes == null || processes.Count < 1)
                return false;

            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i] == null)
                    continue;

                if (processes[i].IsProcessing)
                    return true;
            }

            return false;
        }

        public bool ProcessWasSuccessful(List<BrainProcess> processes)
        {
            if (processes == null || processes.Count < 1)
                return false;

            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i] == null || !processes[i].WasSuccessful)
                    return false;
            }

            return true;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null || _brainAudio == null)
                return Complete(InstructionTileResult.Failure);

            if (!_isRunning)
            {
                _localProcesses.Clear();
                _cloudProcesses.Clear();

                if (!string.IsNullOrEmpty(_saveNowName))
                    _saveNow = _brain.Variables.GetBool(_saveNowName);

                if (UseLocalStorage && _localStorage == null)
                {
                    _localStorage = _globalBrainRunner.LocalStorage;

                    if (_localStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                if (UseCloudStorage && _cloudStorage == null)
                {
                    _cloudStorage = _globalBrainRunner.CloudStorage;

                    if (_cloudStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                StoreVolumeLevel(AudioClassificationType.Master);
                StoreVolumeLevel(AudioClassificationType.SoundEffect);
                StoreVolumeLevel(AudioClassificationType.Voice);
                StoreVolumeLevel(AudioClassificationType.Music);
                StoreVolumeLevel(AudioClassificationType.Ambience);
                StoreVolumeLevel(AudioClassificationType.UserInterface);
                StoreVolumeLevel(AudioClassificationType.Cutscene);
                StoreVolumeLevel(AudioClassificationType.Microphone);
                StoreVolumeLevel(AudioClassificationType.VoiceChat);
                StoreVolumeLevel(AudioClassificationType.Notification);
                StoreVolumeLevel(AudioClassificationType.Miscellaneous);

                _isRunning = true;
                AddFixedTickDelegate();
            }

            if (_localProcesses.Count > 0 || _cloudProcesses.Count > 0)
                return Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);


            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async void StoreVolumeLevel(AudioClassificationType type)
        {
            float valueVolume = _brainAudio.GetUnmodifiedVolumeLevel(type);
            bool valueMuted = _brainAudio.GetVolumeMutedState(type);
            string keyVolume = _keyPrefix;
            string keyMuted = _keyPrefix;

            switch (type)
            {
                case AudioClassificationType.Master:
                    keyVolume += _keyMaster + _keyVolume;
                    keyMuted += _keyMaster + _keyMuted;
                    break;
                case AudioClassificationType.SoundEffect:
                    keyVolume += _keySFX + _keyVolume;
                    keyMuted += _keySFX + _keyMuted;
                    break;
                case AudioClassificationType.Voice:
                    keyVolume += _keyVox + _keyVolume;
                    keyMuted += _keyVox + _keyMuted;
                    break;
                case AudioClassificationType.Music:
                    keyVolume += _keyMusic + _keyVolume;
                    keyMuted += _keyMusic + _keyMuted;
                    break;
                case AudioClassificationType.Ambience:
                    keyVolume += _keyAmbience + _keyVolume;
                    keyMuted += _keyAmbience + _keyMuted;
                    break;
                case AudioClassificationType.UserInterface:
                    keyVolume += _keyUI + _keyVolume;
                    keyMuted += _keyUI + _keyMuted;
                    break;
                case AudioClassificationType.Cutscene:
                    keyVolume += _keyCutscene + _keyVolume;
                    keyMuted += _keyCutscene + _keyMuted;
                    break;
                case AudioClassificationType.Microphone:
                    keyVolume += _keyMic + _keyVolume;
                    keyMuted += _keyMic + _keyMuted;
                    break;
                case AudioClassificationType.VoiceChat:
                    keyVolume += _keyChat + _keyVolume;
                    keyMuted += _keyChat + _keyMuted;
                    break;
                case AudioClassificationType.Notification:
                    keyVolume += _keyNotification + _keyVolume;
                    keyMuted += _keyNotification + _keyMuted;
                    break;
                case AudioClassificationType.Miscellaneous:
                    keyVolume += _keyMisc + _keyVolume;
                    keyMuted += _keyMisc + _keyMuted;
                    break;
            }

            if (UseLocalStorage)
            {
                BrainProcess volumeProcess = await _localStorage.SetFloat(keyVolume, valueVolume, _saveNow);
                BrainProcess mutedProcess = await _localStorage.SetBool(keyMuted, valueMuted, _saveNow);
                _localProcesses.Add(volumeProcess);
                _localProcesses.Add(mutedProcess);
            }

            if (UseCloudStorage)
            {
                BrainProcess volumeProcess = await _cloudStorage.SetFloat(keyVolume, valueVolume, _saveNow);
                BrainProcess mutedProcess = await _cloudStorage.SetBool(keyMuted, valueMuted, _saveNow);
                _cloudProcesses.Add(volumeProcess);
                _cloudProcesses.Add(mutedProcess);
            }
        }
    }
}