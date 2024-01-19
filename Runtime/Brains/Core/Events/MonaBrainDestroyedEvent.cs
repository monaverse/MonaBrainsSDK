using Mona.Brains.Core.Brain;

namespace Mona.Brains.Core.Events
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