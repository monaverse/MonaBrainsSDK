using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Mona.Brains.Core.Brain;
using Mona.Core.Events;
using Mona.Brains.Core.Events;

namespace Mona.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageInstructionTile : InstructionTile, IActionInstructionTile
    {
        public override Type TileType => typeof(BroadcastMessageInstructionTile);

        public BroadcastMessageInstructionTile()
        {
            
        }

        protected void BroadcastMessage(IMonaBrain sender, string message, IMonaBrain target)
        {
            Debug.Log($"{nameof(BroadcastMessage)} '{message}' to ({target.Name}) from ({sender.Name}) on frame {Time.frameCount}");
            EventBus.Trigger<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, target), new MonaBroadcastMessageEvent(message, sender, Time.frameCount));
        }

        public override InstructionTileResult Do()
        {
            throw new NotImplementedException();
        }
    }
}