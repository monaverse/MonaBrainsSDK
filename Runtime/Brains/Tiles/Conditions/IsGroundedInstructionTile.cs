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
    public class IsGroundedInstructionTile : InstructionTile, IInstructionTileWithPreload,
        IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile,
        ITickAfterInstructionTile
    {
        public const string ID = "IsGrounded";
        public const string NAME = "Is Grounded";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(IsGroundedInstructionTile);

        private IMonaBrain _brain;

        public IsGroundedInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && _brain.Body.Grounded)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}