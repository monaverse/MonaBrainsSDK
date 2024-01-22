using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaTriggerEvent : IInstructionEvent
    {
        public MonaTriggerType Type;

        public MonaTriggerEvent(MonaTriggerType type)
        {
            Type = type;
        }
    }
}