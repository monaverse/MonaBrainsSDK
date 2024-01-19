using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.General.Interfaces
{
    public interface ICopyResultInstructionTile : IInstructionTileWithPreload
    {
        string TargetValue { get; set; }
        MonaBrainResultType Source { get; set; }
    }
}