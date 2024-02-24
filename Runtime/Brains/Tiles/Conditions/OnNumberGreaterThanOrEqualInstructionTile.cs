using System;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberGreaterThanOrEqualInstructionTile : OnNumberEqualInstructionTile
    {
        public new const string ID = "OnNumberGreaterThanOrEqual";
        public new const string NAME = "Number Greater Than Or Equal";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(OnNumberGreaterThanOrEqualInstructionTile);

        protected override ValueOperatorType GetOperator()
        {
            return ValueOperatorType.GreaterThanEqual;
        }

        public OnNumberGreaterThanOrEqualInstructionTile() { }

    }
}