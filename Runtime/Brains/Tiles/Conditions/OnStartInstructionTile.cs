using Mona.Brains.Core;
using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Interfaces;
using System;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnStartInstructionTile : InstructionTile, IOnStartInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnStart";
        public const string NAME = "On Start";
        public const string CATEGORY = "Condition";
        public override Type TileType => typeof(OnStartInstructionTile);

        private IMonaBrain _brain;

        public OnStartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain.State != null && _brain.State.GetBool(MonaBrainConstants.ON_STARTING))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_STARTED);
        }
    }
}