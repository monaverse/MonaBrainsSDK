using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainEvent : IInstructionEvent
    {
        public MonaBrainEventType Type;

        public MonaBrainEvent(MonaBrainEventType type)
        {
            Type = type;
        }
    }
}