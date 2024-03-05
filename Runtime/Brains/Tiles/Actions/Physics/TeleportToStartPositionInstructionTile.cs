using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToStartPositionInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToStartPositionInstructionTile";
        public const string NAME = "Teleport To Start Instruction Position";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToStartPositionInstructionTile);

        private IMonaBrain _brain;

        public TeleportToStartPositionInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        } 

        public override InstructionTileResult Do()
        {
            if (_brain != null)
            {
                _brain.Body.TeleportPosition(_brain.Body.InitialPosition, true);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}