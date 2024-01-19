using System;
using System.Collections.Generic;

namespace Mona.Brains.Core.Brain
{
    public interface IMonaBrainRunner
    {
        event Action<IMonaBrainRunner> OnBegin;

        List<IMonaBrain> BrainInstances { get; }
    }
}
