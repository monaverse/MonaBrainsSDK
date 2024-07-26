using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class AddToNumberInstructionTile : SetNumberToInstructionTile
    {
        public override Type TileType => typeof(AddToNumberInstructionTile);

        public AddToNumberInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return ValueChangeType.Add;
        }
    }
}