using UnityEngine;
using System;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using Mona.Brains.Tiles.Conditions.Interfaces;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnMessageInstructionTile : InstructionTile, IOnMessageInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnMessage";
        public const string NAME = "OnMessage";
        public const string CATEGORY = "Condition";
        public override Type TileType => typeof(OnMessageInstructionTile);

        [SerializeField]
        private string _message;

        [BrainProperty(true)]
        public string Message { get => _message; set => _message = value; }

        private IMonaBrain _brain;

        public OnMessageInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        private void SetSender(string message)
        {
            var msg = _brain.GetMessage(message);
            _brain.State.Set(MonaBrainConstants.RESULT_SENDER, msg.Sender);
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && _brain.HasMessage(_message))
            {
                SetSender(_message);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_MESSAGE);
        }
    }
}