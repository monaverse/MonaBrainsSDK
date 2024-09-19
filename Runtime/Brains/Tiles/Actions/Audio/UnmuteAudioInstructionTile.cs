using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Tiles.Actions.Audio
{
    [Serializable]
    public class UnmuteAudioInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        ITickAfterInstructionTile
    {
        public const string ID = "UnmuteAudio";
        public const string NAME = "Unmute Audio";
        public const string CATEGORY = "Audio";
        public override Type TileType => typeof(UnmuteAudioInstructionTile);

        public UnmuteAudioInstructionTile() { }

        [SerializeField] private AudioClassificationType _audioType = AudioClassificationType.Master;
        [BrainPropertyEnum(true)] public AudioClassificationType AudioType { get => _audioType; set => _audioType = value; }

        private IMonaBrain _brain;
        private MonaBrainAudio _brainAudio;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _brainAudio = MonaBrainAudio.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _brainAudio == null)
                return Complete(InstructionTileResult.Failure);

            _brainAudio.SetVolumeMuteState(_audioType, false);

            return Complete(InstructionTileResult.Success);
        }
    }
}