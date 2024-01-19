using Mona.Brains.Core.Tiles;

namespace Mona.Brains.Tiles.Actions.General.Interfaces
{
    public interface ILogInstructionTile : IInstructionTile
    {
        string Message { get; set; }
    }
}