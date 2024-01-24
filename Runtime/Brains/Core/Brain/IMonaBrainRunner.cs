using Mona.SDK.Brains.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrainRunner
    {
        event Action<IMonaBrainRunner> OnBegin;
        void WaitFrame(Action<IInstructionEvent> callback, IInstructionEvent evt, Type type);

        List<IMonaBrain> BrainInstances { get; }
    }
}
