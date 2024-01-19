using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.State.Structs
{
    public interface IMonaStateBrainValue
    { 
        IMonaBrain Value { get; set; }
    }
}