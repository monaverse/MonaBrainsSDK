using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class DivideNumberInstructionTile : SetNumberToInstructionTile
    {
        public new const string ID = "DivideNumber";
        public new const string NAME = "Divide Number";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(DivideNumberInstructionTile);

        public DivideNumberInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return ValueChangeType.Divide;
        }
    }
}