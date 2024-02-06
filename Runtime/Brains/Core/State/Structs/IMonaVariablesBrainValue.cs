using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.State.Structs
{
    public interface IMonaVariablesBrainValue
    { 
        IMonaBrain Value { get; set; }
    }
}