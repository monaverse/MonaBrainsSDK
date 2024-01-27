using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Control
{
    public interface IMonaBrainPage
    {
        List<IInstruction> Instructions { get; }
        string Name { get; set; }
        bool IsCore { get; set; }
        void Preload(IMonaBrain brain);
        void ExecuteInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null);
        void Unload();
        void Pause();
        void Resume();
    }
}