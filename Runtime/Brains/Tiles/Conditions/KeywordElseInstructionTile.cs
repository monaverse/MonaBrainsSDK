using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class KeywordElseInstructionTile : InstructionTile, IKeywordElseInstructionTile, IConditionInstructionTile, IStartableInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "KeywordElse";
        public const string NAME = "ELSE";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(KeywordElseInstructionTile);

        public KeywordElseInstructionTile() { }

        public override InstructionTileResult Do()
        {
            return Complete(InstructionTileResult.Success);
        }
    }
}