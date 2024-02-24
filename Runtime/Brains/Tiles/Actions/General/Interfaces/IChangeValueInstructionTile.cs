using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeValueInstructionTile : IInstructionTileWithPreload
    {
        string ValueName { get; set; }
        ValueChangeType Operator { get; set; }
        float Amount { get; set; }
    }
}