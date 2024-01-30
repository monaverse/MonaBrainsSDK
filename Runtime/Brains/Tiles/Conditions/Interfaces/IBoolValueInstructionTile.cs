using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IBoolValueInstructionTile : IInstructionTileWithPreload
    {
        string ValueName { get; set; }
        BoolValueOperatorType Operator { get; set; }
        bool Value { get; set; }
    }
}