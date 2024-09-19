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
    public class GetVolumeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        ITickAfterInstructionTile
    {
        public const string ID = "GetVolume";
        public const string NAME = "Get Volume";
        public const string CATEGORY = "Audio";
        public override Type TileType => typeof(GetVolumeInstructionTile);

        public GetVolumeInstructionTile() { }

        [SerializeField] private AudioClassificationType _audioType = AudioClassificationType.Master;
        [BrainPropertyEnum(true)] public AudioClassificationType AudioType { get => _audioType; set => _audioType = value; }

        [SerializeField] private OperationType _operation = OperationType.Volume;
        [BrainPropertyEnum(true)] public OperationType Operation { get => _operation; set => _operation = value; }

        [SerializeField] private string _volume;
        [BrainPropertyShow(nameof(Operation), (int)OperationType.Volume)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string Volume { get => _volume; set => _volume = value; }

        [SerializeField] private string _muteState;
        [BrainPropertyShow(nameof(Operation), (int)OperationType.MuteState)]
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), true)] public string MuteState { get => _muteState; set => _muteState = value; }

        private IMonaBrain _brain;
        private MonaBrainAudio _brainAudio;

        public enum OperationType
        {
            Volume,
            MuteState
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

            if (_operation == OperationType.Volume)
            {
                if (string.IsNullOrEmpty(_volume))
                    return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

                float volumeLevel = _brainAudio.GetVolumeLevel(_audioType);
                _brain.Variables.Set(_volume, volumeLevel);
            }
            else
            {
                if (string.IsNullOrEmpty(_muteState))
                    return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

                bool mutedState = _brainAudio.GetVolumeMutedState(_audioType);
                _brain.Variables.Set(_muteState, mutedState);
            }
            
            return Complete(InstructionTileResult.Success);
        }
    }
}