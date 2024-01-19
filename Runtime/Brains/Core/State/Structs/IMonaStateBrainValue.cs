using Mona.Brains.Core.Brain;

namespace Mona.Brains.Core.State.Structs
{
    public interface IMonaStateBrainValue
    { 
        IMonaBrain Value { get; set; }
    }
}