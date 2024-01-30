using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.General.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeBoolValueInstructionTile : IInstructionTileWithPreload
    {
        string ValueName { get; set; }
        bool Value { get; set; }
    }
}