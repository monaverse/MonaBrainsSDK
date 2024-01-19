using Mona.Brains.Core;
using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using Mona.Brains.Tiles.Actions.Broadcasting.Interfaces;
using System;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToSelfInstructionTile : BroadcastMessageInstructionTile, IBroadcastMessageToSelfInstructionTile
    {
        public const string ID = "BroadcastMessageToSelf";
        public const string NAME = "Broadcast Message\n To Self";
        public const string CATEGORY = "Action/Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToSelfInstructionTile);

        [SerializeField]
        private string _message;
        [BrainProperty]
        public string Message { get => _message; set => _message = value; }

        private IMonaBrain _brain;

        public BroadcastMessageToSelfInstructionTile()
        {
        }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            BroadcastMessage(_brain, _message, _brain);
            return Complete(InstructionTileResult.Success);
        }
    }
}