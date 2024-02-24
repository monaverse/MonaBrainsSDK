using System;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class AddToNumberInstructionTile : SetNumberToInstructionTile
    {
        public const string ID = "AddToNumber";
        public const string NAME = "Add To Number";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(AddToNumberInstructionTile);

        public AddToNumberInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return ValueChangeType.Add;
        }
    }
}