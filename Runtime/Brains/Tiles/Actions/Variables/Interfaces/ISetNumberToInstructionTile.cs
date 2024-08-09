using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces
{
    public interface ISetNumberToInstructionTile : IInstructionTileWithPreloadAndPageAndInstruction
    { 
        string NumberName { get; set; }
        float Amount { get; set; }
    }
}