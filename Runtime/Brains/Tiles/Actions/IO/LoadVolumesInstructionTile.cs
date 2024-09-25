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
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LoadVolumesInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "LoadVolumes";
        public const string NAME = "Load Volumes";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadVolumesInstructionTile);

        [SerializeField] private StorageTargetType _storageTarget = StorageTargetType.LocalAndCloud;
        [BrainPropertyEnum(true)] public StorageTargetType StorageTarget { get => _storageTarget; set => _storageTarget = value; }

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

        public LoadVolumesInstructionTile() { }

        private bool _mixProcessed;
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
            bool localSuccess = UseLocalStorage ? ProcessWasSuccessful(_localProcesses) : false;
            bool cloudSuccess = UseCloudStorage ? ProcessWasSuccessful(_cloudProcesses) : false;

            if (localSuccess) SetGameVolumes(_localProcesses);
            else if (cloudSuccess) SetGameVolumes(_cloudProcesses);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
            {
                bool success = UseLocalStorage && localSuccess || (UseCloudStorage && cloudSuccess);
                _brain.Variables.Set(_storeSuccessOn, success);
            }

            _localProcesses.Clear();
            _cloudProcesses.Clear();

            Complete(InstructionTileResult.Success, true);
        }

        private void SetGameVolumes(List<BrainProcess> processes)
        {
            _brainAudio.SetVolumeLevel(AudioClassificationType.Master, processes[0].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Master, processes[1].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.SoundEffect, processes[2].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.SoundEffect, processes[3].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Voice, processes[4].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Voice, processes[5].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Music, processes[6].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Music, processes[7].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Ambience, processes[8].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Ambience, processes[9].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.UserInterface, processes[10].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.UserInterface, processes[11].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Cutscene, processes[12].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Cutscene, processes[13].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Microphone, processes[14].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Microphone, processes[15].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.VoiceChat, processes[16].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.VoiceChat, processes[17].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Notification, processes[18].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Notification, processes[19].GetBool());
            _brainAudio.SetVolumeLevel(AudioClassificationType.Miscellaneous, processes[20].GetFloat());
            _brainAudio.SetVolumeMuteState(AudioClassificationType.Miscellaneous, processes[21].GetBool());
        }

        private bool ProcessIsActive(List<BrainProcess> processes)
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

        private bool ProcessWasSuccessful(List<BrainProcess> processes)
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
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _mixProcessed = false;

            if (!_isRunning)
            {
                _localProcesses.Clear();
                _cloudProcesses.Clear();

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

                ProcessMix();

                _isRunning = true;
                if(!_mixProcessed)
                    AddFixedTickDelegate();
            }

            if (_localProcesses.Count > 0 || _cloudProcesses.Count > 0)
                return _mixProcessed ? Complete(InstructionTileResult.Success) : Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async Task ProcessMix()
        {

            await LoadVolumeLevel(AudioClassificationType.Master);
            await LoadVolumeLevel(AudioClassificationType.SoundEffect);
            await LoadVolumeLevel(AudioClassificationType.Voice);
            await LoadVolumeLevel(AudioClassificationType.Music);
            await LoadVolumeLevel(AudioClassificationType.Ambience);
            await LoadVolumeLevel(AudioClassificationType.UserInterface);
            await LoadVolumeLevel(AudioClassificationType.Cutscene);
            await LoadVolumeLevel(AudioClassificationType.Microphone);
            await LoadVolumeLevel(AudioClassificationType.VoiceChat);
            await LoadVolumeLevel(AudioClassificationType.Notification);
            await LoadVolumeLevel(AudioClassificationType.Miscellaneous);
            
            _mixProcessed = true;
        }

        private async Task LoadVolumeLevel(AudioClassificationType type)
        {
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
                BrainProcess volumeProcess = await _localStorage.LoadFloat(keyVolume);
                BrainProcess mutedProcess = await _localStorage.LoadBool(keyMuted);
                _localProcesses.Add(volumeProcess);
                _localProcesses.Add(mutedProcess);
            }

            if (UseCloudStorage)
            {
                BrainProcess volumeProcess = await _cloudStorage.LoadFloat(keyVolume);
                BrainProcess mutedProcess = await _cloudStorage.LoadBool(keyMuted);
                _cloudProcesses.Add(volumeProcess);
                _cloudProcesses.Add(mutedProcess);
            }
        }
    }
}