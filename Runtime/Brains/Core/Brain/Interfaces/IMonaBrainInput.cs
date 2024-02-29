using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Enums;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainInput
    {
        MonaInput ProcessInput(bool logOutput, MonaInputType logType, MonaInputState logState);
        void StartListening(IInputInstructionTile tile);
        void StopListening(IInputInstructionTile tile);
        int StartListeningForKey(Key key, IInputInstructionTile tile);
        void StopListeningForKey(Key key, IInputInstructionTile tile);
    }
}