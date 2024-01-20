using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.State;

namespace Mona.SDK.Brains.Core.State
{
    public interface IMonaBrainState : IMonaState
    {
        void Set(string variableName, IMonaBrain value);

        IMonaBrain GetBrain(string variableName);

        void SetNetworkState(INetworkMonaState state);
    }

}