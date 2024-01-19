using Mona.Brains.Core.Brain;
using Mona.Core.State;

namespace Mona.Brains.Core.State
{
    public interface IMonaBrainState : IMonaState
    {
        void Set(string variableName, IMonaBrain value);

        IMonaBrain GetBrain(string variableName);
    }

}