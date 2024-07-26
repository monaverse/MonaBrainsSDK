using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using System;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToSenderInstructionTile : BroadcastMessageInstructionTile, IBroadcastMessageToSenderInstructionTile
    {
        public const string ID = "BroadcastMessageToSender";
        public const string NAME = "Broadcast Message\n To Sender";
        public const string CATEGORY = "Action/Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToSenderInstructionTile);

        [SerializeField]
        private string _message;
        [BrainProperty]
        public string Message { get => _message; set => _message = value; }

        private IMonaBrain _brain;
        
        public BroadcastMessageToSenderInstructionTile()
        {
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var senderAsTarget = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
            if(senderAsTarget != null)
                BroadcastMessage(_brain, _message, senderAsTarget);
            
            return Complete(InstructionTileResult.Success);
        }
    }
}