using System;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberLessThanInstructionTile : OnNumberEqualInstructionTile
    {
        public new const string ID = "OnNumberLessThan";
        public new const string NAME = "Number Less Than";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(OnNumberLessThanInstructionTile);

        protected override ValueOperatorType GetOperator()
        {
            return ValueOperatorType.LessThan;
        }

        public OnNumberLessThanInstructionTile() { }

    }
}