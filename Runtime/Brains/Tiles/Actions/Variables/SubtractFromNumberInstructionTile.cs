using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SubtractFromNumberInstructionTile : SetNumberToInstructionTile
    {
        public new const string ID = "SubtractFromNumber";
        public new const string NAME = "Subtract From Number";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(SubtractFromNumberInstructionTile);

        public SubtractFromNumberInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return ValueChangeType.Subtract;
        }
    }
}