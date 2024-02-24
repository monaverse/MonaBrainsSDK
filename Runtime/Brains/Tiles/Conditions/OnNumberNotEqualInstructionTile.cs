using System;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNumberNotEqualInstructionTile : OnNumberEqualInstructionTile
    {
        public new const string ID = "OnNumberEquals";
        public new const string NAME = "Number Not Equal To";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(OnNumberNotEqualInstructionTile);

        protected override ValueOperatorType GetOperator()
        {
            return ValueOperatorType.NotEqual;
        }

        public OnNumberNotEqualInstructionTile() { }

    }
}