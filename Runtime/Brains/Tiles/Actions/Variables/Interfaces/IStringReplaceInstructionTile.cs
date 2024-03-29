using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface IStringReplaceInstructionTile : IInstructionTileWithPreload
    {
        string StringName { get; set; }
        string ReplaceThis { get; set; }
        string WithThis { get; set; }
    }
}