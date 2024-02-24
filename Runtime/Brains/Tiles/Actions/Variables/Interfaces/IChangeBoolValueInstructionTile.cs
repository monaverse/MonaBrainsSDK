using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface IChangeBoolValueInstructionTile : IInstructionTileWithPreload
    {
        string ValueName { get; set; }
        bool Value { get; set; }
    }
}