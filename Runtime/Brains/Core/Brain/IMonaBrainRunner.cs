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

        bool RequiresAnimator { get; }
        bool LegacyMonaPlatforms { get; }

        void WaitFrame(int brainIndex, Action<IInstructionEvent> callback, IInstructionEvent evt, Type type);
        void WaitFrame(Action callback);

        List<MonaBrainGraph> BrainGraphs { get; }
        void SetBrainGraphs(List<MonaBrainGraph> brainGraphs);
        void AddBrainGraph(MonaBrainGraph brainGraph);

        List<IMonaBrain> BrainInstances { get; }

        bool HasMonaTag(string tag);

        void PreloadBrains();
        void StartBrains();

        void CacheTransforms();
        void ResetTransforms();

    }
}
