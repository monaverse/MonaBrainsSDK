using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    public class StringSubtractInstructionTile : SetStringToInstructionTile
    {
        public override Type TileType => typeof(StringSubtractInstructionTile);

        protected override bool Evaluate(IMonaBrainVariables state)
        {
            var variable = state.GetVariable(_stringName);

            if (variable == null)
            {
                state.Set(_stringName, _value);
                return true;
            }

            if (!(variable is IMonaVariablesStringValue))
                return false;

            string variableValue = ((IMonaVariablesStringValue)variable).Value;

            string subtractedValue = variableValue.Replace(_value, "");
            state.Set(_stringName, subtractedValue);

            return true;
        }
    }
}
