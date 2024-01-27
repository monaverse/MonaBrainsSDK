using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnMessageInstructionTile : InstructionTile, IOnMessageInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnMessage";
        public const string NAME = "Has Message";
        public const string CATEGORY = "General";
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