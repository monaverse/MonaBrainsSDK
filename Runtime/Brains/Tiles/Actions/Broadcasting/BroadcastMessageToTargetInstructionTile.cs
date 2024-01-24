using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
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
            var value = _brain.State.GetValue(_targetValue);
            if (value is IMonaStateBrainValue)
            {
                IMonaBrain targetBrain = ((IMonaStateBrainValue)value).Value;
                if (targetBrain != null)
                    BroadcastMessage(_brain, _message, targetBrain);
            }
            else
            {
                IMonaBody targetBody = ((IMonaStateBodyValue)value).Value;
                if (targetBody != null)
                    BroadcastMessage(_brain, _message, targetBody);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}