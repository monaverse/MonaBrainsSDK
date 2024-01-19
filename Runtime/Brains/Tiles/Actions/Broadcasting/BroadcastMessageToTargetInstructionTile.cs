using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using System;
using UnityEngine;
using Mona.Brains.Core.Brain;
using Mona.Brains.Tiles.Actions.Broadcasting.Interfaces;

namespace Mona.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToTargetInstructionTile : BroadcastMessageInstructionTile, IBroadcastMessageToTargetInstructionTile
    {
        public const string ID = "BroadcastMessageToTarget";
        public const string NAME = "Broadcast Message\n To Target";
        public const string CATEGORY = "Action/Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToTargetInstructionTile);

        [SerializeField] private string _message;
        [BrainProperty(true)] public string Message { get => _message; set => _message = value; }

        [SerializeField] private string _targetValue;
        [BrainProperty(true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }
        
        private IMonaBrain _brain;

        public BroadcastMessageToTargetInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBrain targetBrain = _brain.State.GetBrain(_targetValue);
            
            if (targetBrain != null)
                BroadcastMessage(_brain, _message, targetBrain);

            return Complete(InstructionTileResult.Success);
        }
    }
}