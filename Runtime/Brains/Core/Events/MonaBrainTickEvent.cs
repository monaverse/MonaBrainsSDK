using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainTickEvent : IInstructionEvent
    {
        public InstructionEventTypes Type;
        public MonaBrainTickEvent(InstructionEventTypes type)
        {
            Type = type;
        }
    }
}