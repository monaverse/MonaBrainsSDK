using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnHasControlInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "OnHasControl";
        public const string NAME = "Has Control";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(OnHasControlInstructionTile);

        private IMonaBrain _brain;

        public OnHasControlInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain.Body.HasControl())
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_CONTROLLED);
        }
    }
}