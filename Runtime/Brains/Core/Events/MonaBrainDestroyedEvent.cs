using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainDestroyedEvent
    {
        public IMonaBrain Brain;
        public MonaBrainDestroyedEvent(IMonaBrain brain)
        {
            Brain = brain;
        }
    }
}