using System;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberGreaterThanInstructionTile : OnNumberEqualInstructionTile
    {
        public new const string ID = "OnNumberGreaterThan";
        public new const string NAME = "Number Greater Than";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(OnNumberGreaterThanInstructionTile);

        protected override ValueOperatorType GetOperator()
        {
            return ValueOperatorType.GreaterThan;
        }

        public OnNumberGreaterThanInstructionTile() { }

    }
}