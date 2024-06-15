using Mona.SDK.Brains.Core.Enums;
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

namespace Mona.SDK.Brains.Tiles.Actions.Audio
{
    [Serializable]
    public class StopAudioInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, ITickAfterInstructionTile
    {
        public const string ID = "StopAudio";
        public const string NAME = "Stop Audio";
        public const string CATEGORY = "Audio";
        public override Type TileType => typeof(StopAudioInstructionTile);

        public StopAudioInstructionTile() { }

        private IMonaBrain _brain;

        private AudioSource _audioSource;

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            SetupAudioSource();
        }

        private void SetupAudioSource()
        {
            _audioSource = _brain.Body.ActiveTransform.GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = _brain.Body.ActiveTransform.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.dopplerLevel = 0;
        }

        public override void Unload(bool destroy = false)
        {
        }

        public override InstructionTileResult Do()
        {

            _audioSource.Stop();
            return Complete(InstructionTileResult.Success);
        }
    }
}