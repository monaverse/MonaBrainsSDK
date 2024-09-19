using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Audio
{
    [Serializable]
    public class SetVolumeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        ITickAfterInstructionTile
    {
        public const string ID = "SetVolume";
        public const string NAME = "Set Volume";
        public const string CATEGORY = "Audio";
        public override Type TileType => typeof(SetVolumeInstructionTile);

        public SetVolumeInstructionTile() { }

        [SerializeField] private AudioClassificationType _audioType = AudioClassificationType.Master;
        [BrainPropertyEnum(true)] public AudioClassificationType AudioType { get => _audioType; set => _audioType = value; }

        [SerializeField] private AudioOperation _operation = AudioOperation.SetVolume;
        [BrainPropertyEnum(true)] public AudioOperation Operation { get => _operation; set => _operation = value; }

        [SerializeField] private float _volume = 1f;
        [SerializeField] private string _volumeName;
        [BrainPropertyShow(nameof(Operation), (int)AudioOperation.SetVolume)]
        [BrainProperty(true)] public float Volume { get => _volume; set => _volume = value; }
        [BrainPropertyValueName("Volume", typeof(IMonaVariablesFloatValue))] public string VolumeName { get => _volumeName; set => _volumeName = value; }

        [SerializeField] private float _step = 0.1f;
        [SerializeField] private string _stepName;
        [BrainPropertyShow(nameof(Operation), (int)AudioOperation.IncreaseVolume)]
        [BrainPropertyShow(nameof(Operation), (int)AudioOperation.DecreaseVolume)]
        [BrainProperty(true)] public float Step { get => _step; set => _step = value; }
        [BrainPropertyValueName("Step", typeof(IMonaVariablesFloatValue))] public string StepName { get => _stepName; set => _stepName = value; }

        [SerializeField] private bool _mute;
        [SerializeField] private string _muteName;
        [BrainPropertyShow(nameof(Operation), (int)AudioOperation.MuteState)]
        [BrainProperty(true)] public bool Mute { get => _mute; set => _mute = value; }
        [BrainPropertyValueName("Mute", typeof(IMonaVariablesBoolValue))] public string MuteName { get => _muteName; set => _muteName = value; }

        private IMonaBrain _brain;
        private MonaBrainAudio _brainAudio;

        [Serializable]
        public enum AudioOperation
        {
            SetVolume = 0,
            IncreaseVolume = 10,
            DecreaseVolume = 20,
            MuteState = 30
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _brainAudio = MonaBrainAudio.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _brainAudio == null)
                return Complete(InstructionTileResult.Failure);

            if (!string.IsNullOrEmpty(_volumeName))
                _volume = _brain.Variables.GetFloat(_volumeName);

            if (!string.IsNullOrEmpty(_stepName))
                _step = _brain.Variables.GetFloat(_stepName);

            if (!string.IsNullOrEmpty(_muteName))
                _mute = _brain.Variables.GetBool(_muteName);

            switch (_operation)
            {
                case AudioOperation.SetVolume:
                    _brainAudio.SetVolumeLevel(_audioType, _volume);
                    break;
                case AudioOperation.IncreaseVolume:
                    _brainAudio.AdjustVolumeLevelByStep(_audioType, _step);
                    break;
                case AudioOperation.DecreaseVolume:
                    _brainAudio.AdjustVolumeLevelByStep(_audioType, _step * -1f);
                    break;
                case AudioOperation.MuteState:
                    _brainAudio.SetVolumeMuteState(_audioType, _mute);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}