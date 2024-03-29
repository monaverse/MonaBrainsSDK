using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    public class StringEqualsInstructionTile : SetStringToInstructionTile
    {
        public override Type TileType => typeof(StringEqualsInstructionTile);

        protected override bool Evaluate(IMonaBrainVariables state)
        {
            state.Set(_stringName, _value);
            return true;
        }
    }
}
