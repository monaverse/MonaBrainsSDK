using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IChangeStateInstructionTile : IInstructionTileWithPreloadAndPageAndInstruction
    {
        string State { get; set; }
    }
}