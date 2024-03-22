using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces
{
    public interface IApplyForceLocalInstructionTile : IInstructionTileWithPreloadAndPageAndInstruction
    {
        float Force { get; set; }
        float Duration { get; set; }
    }
}
