using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface ISetStringToInstructionTile : IInstructionTileWithPreload
    {
        string StringName { get; set; }
        string Value { get; set; }
    }
}