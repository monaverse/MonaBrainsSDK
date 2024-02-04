using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Enums;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainInput
    {
        MonaInput ProcessInput(bool logOutput, MonaInputType logType, MonaInputState logState);
        void StartListening(IMonaBody body);
        void StopListening(IMonaBody body);
        int StartListeningForKey(Key key);
        void StopListeningForKey(Key key);
    }
}