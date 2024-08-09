using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface IStoreVariableInstructionTile
    {
        VariableTargetToStoreResult SetResultTo { get; set; }
        public string StoreResultOn { get; set; }
    }
}