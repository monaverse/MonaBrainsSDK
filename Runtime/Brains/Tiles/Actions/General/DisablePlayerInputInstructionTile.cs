using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using Mona.SDK.Brains.Core.Brain;
using Unity.VisualScripting;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Actions.Input
{
    [Serializable]
    public class DisablePlayerInputInstructionTile : InstructionTile, IActionInstructionTile
    {
        public const string ID = "DisablePlayerInput";
        public const string NAME = "Disable Player Input";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(DisablePlayerInputInstructionTile);

        public DisablePlayerInputInstructionTile() { }

        public override InstructionTileResult Do()
        {
            EventBus.Trigger<MonaPlayerInputEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_INPUT_EVENT), new MonaPlayerInputEvent(false));
            return Complete(InstructionTileResult.Success);
        }
    }
}