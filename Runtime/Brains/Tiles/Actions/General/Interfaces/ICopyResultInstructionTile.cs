using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface ICopyResultInstructionTile : IInstructionTileWithPreloadAndPageAndInstruction
    {
        string TargetValue { get; set; }
        MonaBrainResultType Source { get; set; }
    }
}