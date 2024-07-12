using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnIsPlayerInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "OnIsPlayer";
        public const string NAME = "Is Player";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(OnIsPlayerInstructionTile);

        [SerializeField] private float _playerId;
        [SerializeField] private string _playerIdName;
        [BrainProperty(true)] public float PlayerId { get => _playerId; set => _playerId = value; }
        [BrainPropertyValueName(nameof(PlayerId), typeof(IMonaVariablesFloatValue))] public string MyNumberName { get => _playerIdName; set => _playerIdName = value; }

        private IMonaBrain _brain;

        public OnIsPlayerInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            //if(_brain.LoggingEnabled) Debug.Log($"{nameof(OnIsPlayerInstructionTile)} {_brain.Body.HasControl()}", _brain.Body.Transform.gameObject);
            if (MonaGlobalBrainRunner.Instance.PlayerId == (int)_playerId)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_CONTROLLED);
        }
    }
}