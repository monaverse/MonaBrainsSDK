using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrainRunner
    {
        event Action<IMonaBrainRunner> OnBegin;
        void WaitFrame(Action<IInstructionEvent> callback, IInstructionEvent evt, Type type);

        List<MonaBrainGraph> BrainGraphs { get; }
        void SetBrainGraphs(List<MonaBrainGraph> brainGraphs);

        List<IMonaBrain> BrainInstances { get; }

        void StartBrains();
    }
}
