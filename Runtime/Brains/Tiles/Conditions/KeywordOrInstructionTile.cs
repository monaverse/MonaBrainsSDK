using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class KeywordOrInstructionTile : InstructionTile, IKeywordOrInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "KeywordOr";
        public const string NAME = "OR";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(KeywordOrInstructionTile);

        public KeywordOrInstructionTile() { }

        public override InstructionTileResult Do()
        {
            return Complete(InstructionTileResult.Success);
        }
    }
}