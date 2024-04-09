using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageInstructionTile : InstructionTile, IActionInstructionTile
    {
        public override Type TileType => typeof(BroadcastMessageInstructionTile);

        public BroadcastMessageInstructionTile()
        {

        }

        protected void BroadcastMessage(IMonaBrain sender, string message, IMonaBody target)
        {
            //Debug.Log($"{nameof(BroadcastMessage)} '{message}' to ({target.Name}) from ({sender.Name}) on frame {Time.frameCount}");
            EventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, target), new InstructionEvent(message, sender, Time.frameCount));
        }

        protected void BroadcastMessage(IMonaBrain sender, string message, IMonaBrain target)
        {
            //Debug.Log($"{nameof(BroadcastMessage)} '{message}' to ({target.Name}) from ({sender.Name}) on frame {Time.frameCount}");
            EventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, target), new InstructionEvent(message, sender, Time.frameCount));
        }

        public override InstructionTileResult Do()
        {
            throw new NotImplementedException();
        }
    }
}