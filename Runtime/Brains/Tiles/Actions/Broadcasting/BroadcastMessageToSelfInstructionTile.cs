using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;
using Mona.SDK.Core.Body;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
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
            Debug.Log($"{nameof(BroadcastMessageToSelfInstructionTile)} {_message}");
            if (_brain.Body is IMonaBodyPart)
            {
                IMonaBody parent = _brain.Body.Parent;
                if (parent != null)
                    BroadcastMessage(_brain, _message, parent);
            }
            else
            {
                BroadcastMessage(_brain, _message, _brain.Body);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}