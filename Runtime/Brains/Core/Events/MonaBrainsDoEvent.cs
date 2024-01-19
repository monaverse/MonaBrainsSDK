using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainsDoEvent
    {
        public IMonaBrain Brain;
        public string EventName;

        public MonaBrainsDoEvent(IMonaBrain brain, string eventName)
        {
            Brain = brain;
            EventName = eventName;
        }
    }
}