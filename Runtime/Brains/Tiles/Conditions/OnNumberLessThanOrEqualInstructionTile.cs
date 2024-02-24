using System;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberLessThanOrEqualInstructionTile : OnNumberEqualInstructionTile
    {
        public new const string ID = "OnNumberLessThanOrEqual";
        public new const string NAME = "Number Less Than Or Equal";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(OnNumberLessThanOrEqualInstructionTile);

        protected override ValueOperatorType GetOperator()
        {
            return ValueOperatorType.LessThanEqual;
        }

        public OnNumberLessThanOrEqualInstructionTile() { }

    }
}