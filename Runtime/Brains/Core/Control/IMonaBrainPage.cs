using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Control
{
    public interface IMonaBrainPage
    {
        List<IInstruction> Instructions { get; }
        string Name { get; set; }
        void Preload(IMonaBrain brain);
        void ExecuteInstructions(InstructionEventTypes eventType);
        void Unload();
    }
}