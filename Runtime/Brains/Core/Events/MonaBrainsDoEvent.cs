using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;

namespace Mona.Brains.Core.Events
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