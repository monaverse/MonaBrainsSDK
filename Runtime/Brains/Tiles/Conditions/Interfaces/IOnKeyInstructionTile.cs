using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnKeyInstructionTile : IInstructionTile
    {
        Key Key { get; set; }
        MonaInputState InputState { get; set; }
    }
}
