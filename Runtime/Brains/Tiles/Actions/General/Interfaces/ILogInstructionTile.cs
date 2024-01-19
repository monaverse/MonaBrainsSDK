using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface ILogInstructionTile : IInstructionTile
    {
        string Message { get; set; }
    }
}