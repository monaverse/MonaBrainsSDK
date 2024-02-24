using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class MultiplyNumberInstructionTile : SetNumberToInstructionTile
    {
        public new const string ID = "MultiplyNumber";
        public new const string NAME = "Multiply Number";
        public new const string CATEGORY = "Variables";
        public override Type TileType => typeof(MultiplyNumberInstructionTile);

        public MultiplyNumberInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return ValueChangeType.Multiply;
        }
    }
}