using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaTriggerEvent : IInstructionEvent
    {
        public MonaTriggerType Type;
        public IMonaBrainPage Page;

        public MonaTriggerEvent(MonaTriggerType type, IMonaBrainPage page)
        {
            Type = type;
            Page = page;
        }
    }
}