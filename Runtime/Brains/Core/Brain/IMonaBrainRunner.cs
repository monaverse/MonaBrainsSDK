using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Body.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrainRunner
    {
        event Action<IMonaBrainRunner> OnBegin;
        event Action<string> OnMessage;

        IMonaBody Body { get; }

        bool RequiresAnimator { get; }
        bool LegacyMonaPlatforms { get; }

        bool HasRigidbodyTiles();

        void WaitFrame(int brainIndex, Action<InstructionEvent> callback, InstructionEvent evt, bool debug);

        List<MonaBrainGraph> BrainGraphs { get; }
        List<string> BrainUrls { get; }

        void SetBrainGraphs(List<MonaBrainGraph> brainGraphs);
        void AddBrainGraph(MonaBrainGraph brainGraph);

        void LoadBrainGraph(string url, Action<List<IMonaBrain>> callback = null);
        void LoadBrainGraphs(List<string> urls, Action<List<IMonaBrain>> callback = null);

        List<IMonaBrain> BrainInstances { get; }

        bool HasMonaTag(string tag);

        void PreloadBrains();
        void StartBrains(bool force = false);

        void CacheTransforms();
        void ResetTransforms();

        void TriggerMessage(string message);

    }
}
