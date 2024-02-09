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
    public class EnablePlayerInputInstructionTile : InstructionTile, IActionInstructionTile
    {
        public const string ID = "EnablePlayerInput";
        public const string NAME = "Enable Player Input";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(EnablePlayerInputInstructionTile);

        public EnablePlayerInputInstructionTile() { }

        public override InstructionTileResult Do()
        {
            EventBus.Trigger<MonaPlayerInputEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_INPUT_EVENT), new MonaPlayerInputEvent(true));
            return Complete(InstructionTileResult.Success);
        }
    }
}