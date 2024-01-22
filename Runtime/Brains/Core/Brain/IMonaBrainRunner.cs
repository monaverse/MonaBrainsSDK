using Mona.SDK.Brains.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrainRunner
    {
        event Action<IMonaBrainRunner> OnBegin;
        void WaitFrame(Action<IBrainMessageEvent> callback, IBrainMessageEvent evt);

        List<IMonaBrain> BrainInstances { get; }
    }
}
