using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnIsClientInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "OnIsClient";
        public const string NAME = "IsClient";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(OnIsClientInstructionTile);

        private IMonaBrain _brain;

        public OnIsClientInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!MonaGlobalBrainRunner.Instance.IsHost)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_CLIENT);
        }
    }
}