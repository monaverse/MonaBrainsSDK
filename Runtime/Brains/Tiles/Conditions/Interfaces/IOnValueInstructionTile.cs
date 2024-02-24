using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnValueInstructionTile : IInstructionTileWithPreload
    {
        string ValueName { get; set; }
        float Amount { get; set; }
    }
}